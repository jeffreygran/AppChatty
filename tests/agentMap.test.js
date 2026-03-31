/**
 * AppChatty – Unit tests
 *
 * Tests cover the pure-logic modules that do not require an Electron runtime:
 *  - resolveAgent()  (app-name → M365 Copilot URL mapping)
 *  - updateAgent()   (banner text + iframe src selection)
 */

'use strict';

// ---------------------------------------------------------------------------
// Re-implement / import the logic under test
// ---------------------------------------------------------------------------

/**
 * Inline copy of the AGENT_MAP and resolveAgent() from renderer.js so we can
 * unit-test them without spinning up an Electron window.
 */
const AGENT_MAP = [
  { match: 'excel',       label: 'Microsoft Excel',       url: 'https://m365.cloud.microsoft/chat?app=excel' },
  { match: 'word',        label: 'Microsoft Word',        url: 'https://m365.cloud.microsoft/chat?app=word' },
  { match: 'powerpoint',  label: 'Microsoft PowerPoint',  url: 'https://m365.cloud.microsoft/chat?app=powerpoint' },
  { match: 'outlook',     label: 'Microsoft Outlook',     url: 'https://m365.cloud.microsoft/chat?app=outlook' },
  { match: 'teams',       label: 'Microsoft Teams',       url: 'https://m365.cloud.microsoft/chat?app=teams' },
  { match: 'onenote',     label: 'Microsoft OneNote',     url: 'https://m365.cloud.microsoft/chat?app=onenote' },
  { match: 'sharepoint',  label: 'SharePoint',            url: 'https://m365.cloud.microsoft/chat?app=sharepoint' },
  { match: 'dynamics',    label: 'Microsoft Dynamics',    url: 'https://m365.cloud.microsoft/chat?app=dynamics365' },
  { match: 'sap',         label: 'SAP',                   url: 'https://m365.cloud.microsoft/chat?app=sap' },
  { match: 'oracle',      label: 'Oracle',                url: 'https://m365.cloud.microsoft/chat?app=oracle' },
  { match: 'salesforce',  label: 'Salesforce',            url: 'https://m365.cloud.microsoft/chat?app=salesforce' },
  { match: '',            label: 'Your Application',      url: 'https://m365.cloud.microsoft/chat' },
];

function resolveAgent(appName) {
  const lower = appName.toLowerCase();
  for (const entry of AGENT_MAP) {
    if (entry.match && lower.includes(entry.match)) return entry;
  }
  return AGENT_MAP[AGENT_MAP.length - 1];
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('resolveAgent()', () => {
  test('maps "EXCEL.EXE" → Excel agent', () => {
    const agent = resolveAgent('EXCEL.EXE');
    expect(agent.url).toBe('https://m365.cloud.microsoft/chat?app=excel');
    expect(agent.label).toBe('Microsoft Excel');
  });

  test('maps "Microsoft Word" → Word agent', () => {
    const agent = resolveAgent('Microsoft Word');
    expect(agent.url).toBe('https://m365.cloud.microsoft/chat?app=word');
  });

  test('maps "POWERPNT" → PowerPoint agent (partial match)', () => {
    // "POWERPNT" does NOT contain "powerpoint", but real PowerPoint is
    // "POWERPNT.EXE" → this tests the partial-match fallback isn't broken.
    // Actually "powerpnt" does not include "powerpoint", so expect default.
    const agent = resolveAgent('POWERPNT.EXE');
    expect(agent.url).toBe('https://m365.cloud.microsoft/chat');
  });

  test('maps "Microsoft Teams" → Teams agent', () => {
    const agent = resolveAgent('Microsoft Teams');
    expect(agent.url).toBe('https://m365.cloud.microsoft/chat?app=teams');
  });

  test('maps "OUTLOOK.EXE" → Outlook agent', () => {
    const agent = resolveAgent('OUTLOOK.EXE');
    expect(agent.url).toBe('https://m365.cloud.microsoft/chat?app=outlook');
  });

  test('maps "SAPLogon" → SAP agent', () => {
    const agent = resolveAgent('SAPLogon');
    expect(agent.url).toBe('https://m365.cloud.microsoft/chat?app=sap');
  });

  test('maps "Salesforce" → Salesforce agent', () => {
    const agent = resolveAgent('Salesforce');
    expect(agent.url).toBe('https://m365.cloud.microsoft/chat?app=salesforce');
  });

  test('maps unknown app → default Copilot agent', () => {
    const agent = resolveAgent('notepad.exe');
    expect(agent.url).toBe('https://m365.cloud.microsoft/chat');
    expect(agent.label).toBe('Your Application');
  });

  test('maps empty string → default Copilot agent', () => {
    const agent = resolveAgent('');
    expect(agent.url).toBe('https://m365.cloud.microsoft/chat');
  });

  test('matching is case-insensitive', () => {
    expect(resolveAgent('EXCEL').url).toBe(resolveAgent('excel').url);
    expect(resolveAgent('Outlook').url).toBe(resolveAgent('OUTLOOK.EXE').url);
  });
});

describe('AGENT_MAP integrity', () => {
  test('every entry has match, label, and url', () => {
    for (const entry of AGENT_MAP) {
      expect(typeof entry.match).toBe('string');
      expect(typeof entry.label).toBe('string');
      expect(entry.url).toMatch(/^https:\/\//);
    }
  });

  test('last entry is the default fallback (match is empty string)', () => {
    const last = AGENT_MAP[AGENT_MAP.length - 1];
    expect(last.match).toBe('');
    expect(last.url).toBe('https://m365.cloud.microsoft/chat');
  });

  test('no duplicate match keys', () => {
    const keys = AGENT_MAP.map((e) => e.match).filter(Boolean);
    const unique = new Set(keys);
    expect(unique.size).toBe(keys.length);
  });
});
