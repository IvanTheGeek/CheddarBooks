import { defineConfig } from '@playwright/test';
import path from 'node:path';

const workspaceHtmlRoot = path.resolve(__dirname, '..', '..', 'workspace', 'laundrylog', 'html');

export default defineConfig({
  testDir: path.resolve(__dirname, 'specs'),
  fullyParallel: false,
  workers: 1,
  retries: 0,
  reporter: [
    ['list'],
    ['html', { open: 'never', outputFolder: path.resolve(__dirname, 'playwright-report') }],
  ],
  outputDir: path.resolve(__dirname, 'test-results'),
  use: {
    baseURL: 'http://127.0.0.1:4173',
    headless: true,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    viewport: { width: 1600, height: 1000 },
  },
  webServer: {
    command: 'python3 -m http.server 4173 --bind 127.0.0.1',
    cwd: workspaceHtmlRoot,
    url: 'http://127.0.0.1:4173',
    reuseExistingServer: true,
    timeout: 30_000,
  },
});
