import { expect, test } from '@playwright/test';
import path from 'node:path';
import { pathToFileURL } from 'node:url';

const path1HttpPath = '/screen-paths/LaundryLog_PATH1_AppStarted_NeedLocation_FirstEntry.html';
const path1FileUrl = pathToFileURL(
  path.resolve(
    __dirname,
    '..',
    '..',
    '..',
    'workspace',
    'laundrylog',
    'html',
    'screen-paths',
    'LaundryLog_PATH1_AppStarted_NeedLocation_FirstEntry.html',
  ),
).toString();

type PathMetrics = {
  logicalTargets: number[];
  logicalMaxTarget: number;
  visibleStepKeys: string[];
  leadingVisibleStepKey: string | null;
  viewportScrollLeft: number;
  scrollbarScrollLeft: number;
  viewportMaxScrollLeft: number;
  scrollbarMaxScrollLeft: number;
};

async function readPathMetrics(page): Promise<PathMetrics> {
  return page.evaluate(() => {
    const viewport = document.getElementById('ll-path-flow-viewport') as HTMLElement | null;
    const scrollbar = document.getElementById('ll-path-scrollbar') as HTMLElement | null;
    const flow = document.getElementById('ll-path-flow') as HTMLElement | null;
    const steps = Array.from(document.querySelectorAll<HTMLElement>('[data-testid="path-step"]')).filter(
      (step) => !step.hidden,
    );
    const firstOffset = steps.length > 0 ? steps[0].offsetLeft : 0;
    const normalizedTargets = steps.map((step) => Math.max(0, Math.round(step.offsetLeft - firstOffset)));
    const contentRightEdge =
      normalizedTargets.length > 0
        ? Math.max(
            ...steps.map((step, index) => normalizedTargets[index] + Math.round(step.getBoundingClientRect().width)),
          )
        : 0;
    const maxStartIndexCandidate = normalizedTargets.findIndex(
      (target) => contentRightEdge - target <= ((viewport?.clientWidth ?? 0) + 1),
    );
    const maxStartIndex = maxStartIndexCandidate >= 0 ? maxStartIndexCandidate : Math.max(0, normalizedTargets.length - 1);
    const logicalTargets = normalizedTargets.slice(0, maxStartIndex + 1);
    const logicalMaxTarget = logicalTargets.length > 0 ? logicalTargets[logicalTargets.length - 1] : 0;
    const viewportRect = viewport?.getBoundingClientRect();
    const leadingVisibleStep =
      steps.find((step) => {
        if (!viewportRect) {
          return false;
        }

        const rect = step.getBoundingClientRect();
        return rect.right > viewportRect.left + 1 && rect.left >= viewportRect.left - 4;
      }) ??
      steps.find((step) => {
        if (!viewportRect) {
          return false;
        }

        return step.getBoundingClientRect().right > viewportRect.left + 1;
      }) ??
      null;

    return {
      logicalTargets,
      logicalMaxTarget,
      visibleStepKeys: steps.map((step) => step.dataset.stepKey || ''),
      leadingVisibleStepKey: leadingVisibleStep?.dataset.stepKey || null,
      viewportScrollLeft: Math.round(viewport?.scrollLeft ?? 0),
      scrollbarScrollLeft: Math.round(scrollbar?.scrollLeft ?? 0),
      viewportMaxScrollLeft: Math.round(Math.max(0, (flow?.scrollWidth ?? 0) - (viewport?.clientWidth ?? 0))),
      scrollbarMaxScrollLeft: Math.round(Math.max(0, (scrollbar?.scrollWidth ?? 0) - (scrollbar?.clientWidth ?? 0))),
    };
  });
}

async function waitForScrollTarget(page, expectedScrollLeft: number): Promise<PathMetrics> {
  await expect
    .poll(async () => {
      const metrics = await readPathMetrics(page);
      return {
        viewportScrollLeft: metrics.viewportScrollLeft,
        scrollbarScrollLeft: metrics.scrollbarScrollLeft,
      };
    })
    .toEqual({
      viewportScrollLeft: expectedScrollLeft,
      scrollbarScrollLeft: expectedScrollLeft,
    });

  return readPathMetrics(page);
}

async function clickLens(page, lensKey: string): Promise<void> {
  await page.locator(`[data-testid="path-lens-toggle"][data-lens-key="${lensKey}"]`).click();
}

async function clickViewMode(page, viewMode: string): Promise<void> {
  await page.locator(`[data-testid="path-view-toggle"][data-view-mode="${viewMode}"]`).click();
}

async function readBootStatuses(page, stepKey: string): Promise<string[]> {
  return page
    .locator(`[data-testid="path-step"][data-step-key="${stepKey}"] [data-testid="boot-check"]`)
    .evaluateAll((elements) => elements.map((element) => element.getAttribute('data-status') || ''));
}

async function expectStepRemovedFromLayout(page, stepKey: string): Promise<void> {
  const step = page.locator(`[data-testid="path-step"][data-step-key="${stepKey}"]`);
  await expect(step).toBeHidden();
  await expect(step).toHaveAttribute('hidden', '');
  await expect
    .poll(async () =>
      step.evaluate((element) => window.getComputedStyle(element as HTMLElement).display),
    )
    .toBe('none');
}

test.describe('PATH1 screen path workspace artifact', () => {
  test('next and previous move by whole logical columns and keep the rail in sync', async ({ page }) => {
    await page.goto(path1HttpPath);
    await expect(page.getByRole('heading', { name: 'PATH 1: Fresh First Launch -> Need Location -> First Entry' })).toBeVisible();

    const initialMetrics = await readPathMetrics(page);

    expect(initialMetrics.leadingVisibleStepKey).toBe('01-app-started');
    expect(initialMetrics.logicalTargets.length).toBeGreaterThan(1);

    await page.getByTestId('path-nav-next').click();
    const afterNext = await waitForScrollTarget(page, initialMetrics.logicalTargets[1]);

    expect(afterNext.leadingVisibleStepKey).toBe('02-runtime-checks');

    await page.getByTestId('path-nav-previous').click();
    const afterPrevious = await waitForScrollTarget(page, initialMetrics.logicalTargets[0]);

    expect(afterPrevious.leadingVisibleStepKey).toBe('01-app-started');
    await expect(page.getByTestId('path-nav-start')).toBeDisabled();
    await expect(page.getByTestId('path-nav-previous')).toBeDisabled();
  });

  test('go to start and go to end land on the logical path boundaries and keep the rail aligned', async ({ page }) => {
    await page.goto(path1HttpPath);

    const initialMetrics = await readPathMetrics(page);
    await page.getByTestId('path-nav-end').click();

    const atEnd = await waitForScrollTarget(page, initialMetrics.logicalMaxTarget);

    expect(atEnd.viewportScrollLeft).toBe(atEnd.logicalMaxTarget);
    expect(atEnd.scrollbarScrollLeft).toBe(atEnd.logicalMaxTarget);
    await expect(page.getByTestId('path-nav-next')).toBeDisabled();
    await expect(page.getByTestId('path-nav-end')).toBeDisabled();

    await page.getByTestId('path-nav-start').click();
    const backAtStart = await waitForScrollTarget(page, 0);

    expect(backAtStart.leadingVisibleStepKey).toBe('01-app-started');
    await expect(page.getByTestId('path-nav-start')).toBeDisabled();
  });

  test('lens toggles can isolate each lens family and persist the chosen mix across reloads', async ({ page }) => {
    await page.goto(path1HttpPath);

    await clickLens(page, 'app-runtime');
    await clickLens(page, 'screen-path');
    await expect.poll(async () => (await readPathMetrics(page)).visibleStepKeys).toEqual(['01-app-started']);
    await expect(page.locator('[data-testid="path-step"][data-step-key="01-app-started"]')).toBeVisible();
    await expectStepRemovedFromLayout(page, '02-runtime-checks');
    await expectStepRemovedFromLayout(page, '05-need-location');

    await clickLens(page, 'app-runtime');
    await expect
      .poll(async () => (await readPathMetrics(page)).visibleStepKeys)
      .toEqual(['01-app-started', '02-runtime-checks', '03-no-local-session', '04-route-resolved']);
    await expect(page.locator('[data-testid="path-step"][data-step-key="01-app-started"]')).toBeVisible();
    await expect(page.locator('[data-testid="path-step"][data-step-key="02-runtime-checks"]')).toBeVisible();
    await expectStepRemovedFromLayout(page, '05-need-location');

    await clickLens(page, 'application-lifecycle');
    await expect
      .poll(async () => (await readPathMetrics(page)).visibleStepKeys)
      .toEqual(['02-runtime-checks', '03-no-local-session', '04-route-resolved']);
    await expectStepRemovedFromLayout(page, '01-app-started');
    await expect(page.locator('[data-testid="path-step"][data-step-key="02-runtime-checks"]')).toBeVisible();
    await expectStepRemovedFromLayout(page, '05-need-location');

    await clickLens(page, 'screen-path');
    await expect
      .poll(async () => (await readPathMetrics(page)).visibleStepKeys)
      .toEqual([
        '02-runtime-checks',
        '03-no-local-session',
        '04-route-resolved',
        '05-need-location',
        '06-ready-to-set-location',
        '07-entry-form-ready',
        '08-washer-draft',
        '09-logged-success',
      ]);
    await expect(page.locator('[data-testid="path-step"][data-step-key="02-runtime-checks"]')).toBeVisible();
    await expect(page.locator('[data-testid="path-step"][data-step-key="05-need-location"]')).toBeVisible();
    await expect(page.locator('[data-testid="path-step"][data-step-key="07-entry-form-ready"]')).toBeVisible();

    await page.reload();
    await expect
      .poll(async () => (await readPathMetrics(page)).visibleStepKeys)
      .toEqual([
        '02-runtime-checks',
        '03-no-local-session',
        '04-route-resolved',
        '05-need-location',
        '06-ready-to-set-location',
        '07-entry-form-ready',
        '08-washer-draft',
        '09-logged-success',
      ]);

    await expect(page.locator('[data-testid="path-lens-toggle"][data-lens-key="application-lifecycle"]')).not.toHaveClass(
      /is-active/,
    );
    await expect(page.locator('[data-testid="path-lens-toggle"][data-lens-key="app-runtime"]')).toHaveClass(/is-active/);
    await expect(page.locator('[data-testid="path-lens-toggle"][data-lens-key="screen-path"]')).toHaveClass(/is-active/);
    await expectStepRemovedFromLayout(page, '01-app-started');
    await expect(page.locator('[data-testid="path-step"][data-step-key="02-runtime-checks"]')).toBeVisible();
    await expect(page.locator('[data-testid="path-step"][data-step-key="05-need-location"]')).toBeVisible();
  });

  test('view modes change visible metadata density without changing the core path', async ({ page }) => {
    await page.goto(path1HttpPath);

    const pathDocument = page.getByTestId('screen-path-document');
    const scenarioSummary = page.locator('[data-testid="path-scenario-summary"]');
    const scenarioPanel = page.getByTestId('path-scenario-panel');
    const appStartedStep = page.locator('[data-testid="path-step"][data-step-key="01-app-started"]');
    const appStartedNote = appStartedStep.locator('.ll-path-step__note');
    const appStartedMeta = appStartedStep.locator('.ll-path-step__meta');
    const appStartedSurfaceName = appStartedStep.locator('.ll-screen-surface__name');

    await scenarioSummary.click();
    await expect(pathDocument).toHaveAttribute('data-view-mode', 'standard');
    await expect(appStartedNote).toBeVisible();
    await expect(appStartedMeta).toBeVisible();
    await expect(appStartedStep.getByTestId('path-step-changes')).toBeVisible();
    await expect(appStartedSurfaceName).toBeVisible();
    await expect(scenarioPanel).toBeVisible();
    await expect(page.getByText('No saved location is available yet.')).toBeHidden();
    await expect(page.getByText('The splash screen becomes visible as the first surface.')).toBeVisible();

    await clickViewMode(page, 'summary');
    await expect(pathDocument).toHaveAttribute('data-view-mode', 'summary');
    await expect(appStartedNote).toBeHidden();
    await expect(appStartedMeta).toBeHidden();
    await expect(appStartedStep.getByTestId('path-step-changes')).toBeHidden();
    await expect(appStartedSurfaceName).toBeHidden();
    await expect(scenarioPanel).toBeHidden();

    await clickViewMode(page, 'detailed');
    await expect(pathDocument).toHaveAttribute('data-view-mode', 'detailed');
    await expect(appStartedNote).toBeVisible();
    await expect(appStartedMeta).toBeVisible();
    await expect(appStartedStep.getByTestId('path-step-changes')).toBeVisible();
    await expect(appStartedSurfaceName).toBeVisible();
    await expect(scenarioPanel).toBeVisible();
    await expect(page.getByText('No saved location is available yet.')).toBeVisible();
    await expect(page.getByText('Startup/runtime checks must finish before the first usable screen appears.')).toBeVisible();
  });

  test('startup splash steps stay headerless while checkpoint states progress in order', async ({ page }) => {
    await page.goto(path1HttpPath);

    const startupStepKeys = [
      '01-app-started',
      '02-runtime-checks',
      '03-no-local-session',
      '04-route-resolved',
    ];

    for (const stepKey of startupStepKeys) {
      const step = page.locator(`[data-testid="path-step"][data-step-key="${stepKey}"]`);
      await expect(step.locator('.ll-phone-screen--boot')).toBeVisible();
      await expect(step.locator('.ll-header')).toHaveCount(0);
    }

    expect(await readBootStatuses(page, '01-app-started')).toEqual(['active', 'pending', 'pending', 'pending']);
    expect(await readBootStatuses(page, '02-runtime-checks')).toEqual(['complete', 'active', 'pending', 'pending']);
    expect(await readBootStatuses(page, '03-no-local-session')).toEqual(['complete', 'complete', 'active', 'pending']);
    expect(await readBootStatuses(page, '04-route-resolved')).toEqual(['complete', 'complete', 'complete', 'active']);

    await expect(
      page.locator('[data-testid="path-step"][data-step-key="01-app-started"] .ll-boot-state__title'),
    ).toHaveText('Starting LaundryLog');
    await expect(
      page.locator('[data-testid="path-step"][data-step-key="02-runtime-checks"] .ll-boot-state__title'),
    ).toHaveText('Checking startup requirements');
    await expect(
      page.locator('[data-testid="path-step"][data-step-key="03-no-local-session"] .ll-boot-state__title'),
    ).toHaveText('No local session found');
    await expect(
      page.locator('[data-testid="path-step"][data-step-key="04-route-resolved"] .ll-boot-state__title'),
    ).toHaveText('Routing to Need Location');
    await expect(
      page.locator('[data-testid="path-step"][data-step-key="04-route-resolved"]').getByText(
        'Route to Need Location becomes the active checkpoint.',
      ),
    ).toBeVisible();
  });

  test('tracked file artifact still opens directly from file:// with the basic path controls visible', async ({ page }) => {
    await page.goto(path1FileUrl);

    await expect(page.getByRole('heading', { name: 'PATH 1: Fresh First Launch -> Need Location -> First Entry' })).toBeVisible();
    await expect(page.getByTestId('path-nav-start')).toBeVisible();
    await expect(page.getByTestId('path-nav-next')).toBeVisible();
    await expect(page.getByTestId('path-flow-viewport')).toBeVisible();
  });
});
