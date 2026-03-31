/**
 * AppChatty – Main Process
 *
 * Responsibilities:
 *  1. Create a system-tray icon (AppChatty lives here when minimised).
 *  2. Toggle the Copilot-style side panel when the tray icon is clicked.
 *  3. Poll the OS for the currently-active foreground window and pass the
 *     detected application name to the renderer so it can surface the correct
 *     M365 Copilot agent.
 */

'use strict';

const path = require('path');
const {
  app,
  BrowserWindow,
  Tray,
  Menu,
  ipcMain,
  screen,
  nativeImage,
} = require('electron');

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

/** Width of the side panel in pixels. */
const PANEL_WIDTH = 400;

/** How often (ms) to poll for the active foreground window. */
const POLL_INTERVAL_MS = 2000;

// ---------------------------------------------------------------------------
// Module-level state
// ---------------------------------------------------------------------------

/** @type {Tray|null} */
let tray = null;

/** @type {BrowserWindow|null} */
let panelWindow = null;

/** @type {string} Name of the last detected foreground application. */
let lastDetectedApp = '';

/** @type {ReturnType<typeof setInterval>|null} */
let pollTimer = null;

// ---------------------------------------------------------------------------
// Active-window detection
// ---------------------------------------------------------------------------

/**
 * Returns the name of the currently-active foreground application.
 *
 * On Windows we prefer the `active-win` npm package when available.  Because
 * `active-win` is an optional native dependency we fall back to a PowerShell
 * one-liner so the app keeps working even without it.
 *
 * @returns {Promise<string>} The process / application name, e.g. "Excel".
 */
async function getActiveAppName() {
  // Try active-win first (optional native dep).
  try {
    const activeWin = require('active-win'); // optional native dep
    const info = await activeWin();
    if (info && info.owner && info.owner.name) {
      return info.owner.name;
    }
  } catch (_) {
    // Not installed – fall through to the PowerShell fallback.
  }

  // PowerShell fallback (Windows only).
  if (process.platform === 'win32') {
    const { execFile } = require('child_process');
    return new Promise((resolve) => {
      execFile(
        'powershell',
        [
          '-NoProfile',
          '-NonInteractive',
          '-Command',
          '(Get-Process -Id (Get-WmiObject -Class Win32_ForegroundWindow).ProcessId).Name',
        ],
        { timeout: 3000 },
        (err, stdout) => {
          if (err || !stdout.trim()) {
            resolve('');
          } else {
            resolve(stdout.trim());
          }
        }
      );
    });
  }

  return '';
}

// ---------------------------------------------------------------------------
// Window helpers
// ---------------------------------------------------------------------------

/**
 * Builds and returns the BrowserWindow that acts as the Copilot side panel.
 * The window is positioned on the right edge of the primary display.
 */
function createPanelWindow() {
  const { width: screenWidth, height: screenHeight } =
    screen.getPrimaryDisplay().workAreaSize;

  const win = new BrowserWindow({
    width: PANEL_WIDTH,
    height: screenHeight,
    x: screenWidth - PANEL_WIDTH,
    y: 0,
    frame: false,
    transparent: false,
    resizable: false,
    skipTaskbar: true,
    alwaysOnTop: true,
    show: false,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false,
    },
  });

  win.loadFile(path.join(__dirname, 'renderer', 'index.html'));

  // Hide instead of close so reopening is instant.
  win.on('close', (event) => {
    event.preventDefault();
    win.hide();
  });

  return win;
}

/**
 * Toggles visibility of the side panel.
 * If the panel does not exist yet it is created first.
 */
function togglePanel() {
  if (!panelWindow || panelWindow.isDestroyed()) {
    panelWindow = createPanelWindow();
  }

  if (panelWindow.isVisible()) {
    panelWindow.hide();
  } else {
    // Re-position in case the screen resolution changed.
    const { width: screenWidth, height: screenHeight } =
      screen.getPrimaryDisplay().workAreaSize;
    panelWindow.setBounds({
      x: screenWidth - PANEL_WIDTH,
      y: 0,
      width: PANEL_WIDTH,
      height: screenHeight,
    });
    panelWindow.show();
    panelWindow.focus();
  }
}

// ---------------------------------------------------------------------------
// Tray helpers
// ---------------------------------------------------------------------------

/**
 * Builds the tray icon image.
 * Uses `assets/icon.png` when present; falls back to a programmatically
 * generated 16×16 placeholder so the app always starts.
 */
function buildTrayIcon() {
  const iconPath = path.join(__dirname, 'assets', 'icon.png');
  try {
    const img = nativeImage.createFromPath(iconPath);
    if (!img.isEmpty()) return img;
  } catch (_) {
    // Ignore – use fallback below.
  }

  // Generate a simple solid-colour placeholder (16×16 RGBA).
  const size = 16;
  const buf = Buffer.alloc(size * size * 4);
  for (let i = 0; i < size * size; i++) {
    const offset = i * 4;
    buf[offset] = 0x00;     // R
    buf[offset + 1] = 0x78; // G (blue-ish brand colour)
    buf[offset + 2] = 0xd4; // B
    buf[offset + 3] = 0xff; // A
  }
  return nativeImage.createFromBuffer(buf, { width: size, height: size });
}

/**
 * Creates the system-tray icon and wires up its context menu.
 */
function createTray() {
  const icon = buildTrayIcon();
  tray = new Tray(icon);
  tray.setToolTip('AppChatty – Click to open Copilot panel');

  const contextMenu = Menu.buildFromTemplate([
    {
      label: 'Open Panel',
      click: () => togglePanel(),
    },
    { type: 'separator' },
    {
      label: 'Quit AppChatty',
      click: () => {
        // Allow the window to actually close now.
        if (panelWindow && !panelWindow.isDestroyed()) {
          panelWindow.removeAllListeners('close');
          panelWindow.close();
        }
        app.quit();
      },
    },
  ]);

  tray.setContextMenu(contextMenu);
  tray.on('click', () => togglePanel());
  tray.on('double-click', () => togglePanel());
}

// ---------------------------------------------------------------------------
// Active-window polling
// ---------------------------------------------------------------------------

/**
 * Starts polling the OS for the active foreground window.
 * Whenever the active app changes, the new name is sent to the renderer.
 */
function startPolling() {
  pollTimer = setInterval(async () => {
    const appName = await getActiveAppName();
    if (appName && appName !== lastDetectedApp) {
      lastDetectedApp = appName;
      if (panelWindow && !panelWindow.isDestroyed()) {
        panelWindow.webContents.send('active-app-changed', appName);
      }
    }
  }, POLL_INTERVAL_MS);
}

// ---------------------------------------------------------------------------
// IPC handlers
// ---------------------------------------------------------------------------

/** Renderer → Main: close / hide the panel. */
ipcMain.on('hide-panel', () => {
  if (panelWindow && !panelWindow.isDestroyed()) {
    panelWindow.hide();
  }
});

/** Renderer → Main: request the last-known active app name. */
ipcMain.handle('get-active-app', () => lastDetectedApp);

// ---------------------------------------------------------------------------
// App lifecycle
// ---------------------------------------------------------------------------

// Prevent a second instance from opening a new window.
const gotLock = app.requestSingleInstanceLock();
if (!gotLock) {
  app.quit();
} else {
  app.on('second-instance', () => {
    togglePanel();
  });
}

app.whenReady().then(() => {
  // Prevent a Dock icon on macOS (app is tray-only).
  if (app.dock) app.dock.hide();

  createTray();
  startPolling();

  // Show the panel immediately on first launch so the user sees something.
  panelWindow = createPanelWindow();
  panelWindow.show();
});

// On macOS the app stays alive even with no windows open.
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});

app.on('before-quit', () => {
  if (pollTimer) clearInterval(pollTimer);
});
