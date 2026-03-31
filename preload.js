/**
 * AppChatty – Preload Script
 *
 * Exposes a narrow, safe API surface to the renderer via contextBridge.
 * The renderer never gets direct access to Node.js or Electron internals.
 */

'use strict';

const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('appChatty', {
  /**
   * Ask the main process to hide the panel.
   */
  hidePanel() {
    ipcRenderer.send('hide-panel');
  },

  /**
   * Request the currently-active app name from the main process.
   * @returns {Promise<string>}
   */
  getActiveApp() {
    return ipcRenderer.invoke('get-active-app');
  },

  /**
   * Subscribe to active-app-changed notifications pushed by the main process.
   * @param {(appName: string) => void} callback
   * @returns {() => void} Unsubscribe function.
   */
  onActiveAppChanged(callback) {
    const handler = (_event, appName) => callback(appName);
    ipcRenderer.on('active-app-changed', handler);
    return () => ipcRenderer.removeListener('active-app-changed', handler);
  },
});
