/**
 * AppChatty – Renderer Script
 *
 * Handles:
 *  1. Mapping the active foreground application to an M365 Copilot agent URL.
 *  2. Updating the active-app banner and loading the agent iframe.
 *  3. Header button interactions (close, refresh).
 */

'use strict';

// ---------------------------------------------------------------------------
// App → M365 Copilot agent URL mapping
// ---------------------------------------------------------------------------

/**
 * Maps a (lowercase) application / process name fragment to the URL of the
 * corresponding M365 Copilot agent.
 *
 * Keys are matched with String.prototype.includes() against the lower-cased
 * active app name, so partial matches work (e.g. "excel" matches "EXCEL.EXE").
 *
 * @type {Array<{match: string, label: string, url: string}>}
 */
const AGENT_MAP = [
  {
    match: 'excel',
    label: 'Microsoft Excel',
    url: 'https://m365.cloud.microsoft/chat?app=excel',
  },
  {
    match: 'word',
    label: 'Microsoft Word',
    url: 'https://m365.cloud.microsoft/chat?app=word',
  },
  {
    match: 'powerpoint',
    label: 'Microsoft PowerPoint',
    url: 'https://m365.cloud.microsoft/chat?app=powerpoint',
  },
  {
    match: 'outlook',
    label: 'Microsoft Outlook',
    url: 'https://m365.cloud.microsoft/chat?app=outlook',
  },
  {
    match: 'teams',
    label: 'Microsoft Teams',
    url: 'https://m365.cloud.microsoft/chat?app=teams',
  },
  {
    match: 'onenote',
    label: 'Microsoft OneNote',
    url: 'https://m365.cloud.microsoft/chat?app=onenote',
  },
  {
    match: 'sharepoint',
    label: 'SharePoint',
    url: 'https://m365.cloud.microsoft/chat?app=sharepoint',
  },
  {
    match: 'dynamics',
    label: 'Microsoft Dynamics',
    url: 'https://m365.cloud.microsoft/chat?app=dynamics365',
  },
  {
    match: 'sap',
    label: 'SAP',
    url: 'https://m365.cloud.microsoft/chat?app=sap',
  },
  {
    match: 'oracle',
    label: 'Oracle',
    url: 'https://m365.cloud.microsoft/chat?app=oracle',
  },
  {
    match: 'salesforce',
    label: 'Salesforce',
    url: 'https://m365.cloud.microsoft/chat?app=salesforce',
  },
  // Default / fallback – generic M365 Copilot chat.
  {
    match: '',
    label: 'Your Application',
    url: 'https://m365.cloud.microsoft/chat',
  },
];

/**
 * Resolves the best-matching agent entry for the given application name.
 *
 * @param {string} appName
 * @returns {{ match: string, label: string, url: string }}
 */
function resolveAgent(appName) {
  const lower = appName.toLowerCase();
  for (const entry of AGENT_MAP) {
    if (entry.match && lower.includes(entry.match)) {
      return entry;
    }
  }
  // Return the default entry (last item, match: '').
  return AGENT_MAP[AGENT_MAP.length - 1];
}

// ---------------------------------------------------------------------------
// DOM helpers
// ---------------------------------------------------------------------------

const bannerName = /** @type {HTMLElement} */ (
  document.getElementById('app-banner-name')
);
const agentFrame = /** @type {HTMLIFrameElement} */ (
  document.getElementById('agent-frame')
);
const loadingOverlay = /** @type {HTMLElement} */ (
  document.getElementById('loading-overlay')
);
const loadingMessage = /** @type {HTMLElement} */ (
  document.getElementById('loading-message')
);
const btnClose = /** @type {HTMLButtonElement} */ (
  document.getElementById('btn-close')
);
const btnRefresh = /** @type {HTMLButtonElement} */ (
  document.getElementById('btn-refresh')
);

// ---------------------------------------------------------------------------
// State
// ---------------------------------------------------------------------------

/** URL currently loaded in the iframe. */
let currentAgentUrl = '';

// ---------------------------------------------------------------------------
// Functions
// ---------------------------------------------------------------------------

/**
 * Updates the banner and loads the correct M365 Copilot agent for the given
 * application name. Skips the reload if the agent URL has not changed.
 *
 * @param {string} appName
 */
function updateAgent(appName) {
  const agent = resolveAgent(appName || '');

  // Update the banner.
  bannerName.textContent = appName || 'Unknown Application';

  // Only reload if the agent URL actually changed.
  if (agent.url === currentAgentUrl) return;

  currentAgentUrl = agent.url;
  showLoading(`Loading ${agent.label} Copilot agent…`);
  agentFrame.src = agent.url;
}

/**
 * Shows the loading overlay with the given message.
 * @param {string} message
 */
function showLoading(message) {
  loadingMessage.textContent = message;
  loadingOverlay.classList.remove('hidden');
}

/** Hides the loading overlay. */
function hideLoading() {
  loadingOverlay.classList.add('hidden');
}

// ---------------------------------------------------------------------------
// Event wiring
// ---------------------------------------------------------------------------

// Hide the loading overlay once the iframe content has loaded.
agentFrame.addEventListener('load', () => {
  if (agentFrame.src) hideLoading();
});

// Close / hide the panel.
btnClose.addEventListener('click', () => {
  window.appChatty.hidePanel();
});

// Reload the current agent.
btnRefresh.addEventListener('click', () => {
  if (agentFrame.src) {
    showLoading('Refreshing…');
    agentFrame.contentWindow.location.reload();
  }
});

// Listen for active-app changes pushed by the main process.
window.appChatty.onActiveAppChanged((appName) => {
  updateAgent(appName);
});

// ---------------------------------------------------------------------------
// Initialisation
// ---------------------------------------------------------------------------

(async () => {
  // Request the currently-active app at startup so we load the right agent
  // immediately without waiting for the first poll cycle.
  const initialApp = await window.appChatty.getActiveApp();
  updateAgent(initialApp || '');
})();
