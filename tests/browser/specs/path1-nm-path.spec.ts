import { expect, test, type Page } from '@playwright/test';
import type { Locator } from '@playwright/test';
import path from 'node:path';
import { pathToFileURL } from 'node:url';

const nmPathHttpPath = '/nm-paths/LaundryLog_PATH1_NM_FirstLaunch_FirstEntry.html';
const nmPathFileUrl = pathToFileURL(
  path.resolve(
    __dirname,
    '..',
    '..',
    '..',
    'workspace',
    'laundrylog',
    'html',
    'nm-paths',
    'LaundryLog_PATH1_NM_FirstLaunch_FirstEntry.html',
  ),
).toString();

type NmPathMetrics = {
  logicalTargets: number[];
  logicalStartKeys: string[];
  logicalMaxTarget: number;
  visibleColumnKeys: string[];
  leadingVisibleColumnKey: string | null;
  viewportScrollLeft: number;
  scrollbarScrollLeft: number;
};

type NmPreviewMetrics = {
  columnHeight: number;
  frameWidth: number;
  frameHeight: number;
  activatorHeight: number;
  thumbnailWidth: number;
  thumbnailHeight: number;
  renderingWidth: number;
  renderingHeight: number;
};

type NmSlotHeights = {
  header: number;
  screen: number;
  primary: number;
  secondary: number;
  gwt: number;
};

async function readNmPathMetrics(page: Page): Promise<NmPathMetrics> {
  return page.evaluate(() => {
    const viewport = document.getElementById('nm-path-flow-viewport') as HTMLElement | null;
    const scrollbar = document.getElementById('nm-path-scrollbar') as HTMLElement | null;
    const flow = document.getElementById('nm-path-flow') as HTMLElement | null;
    const columns = Array.from(document.querySelectorAll<HTMLElement>('[data-testid="nm-path-column"]')).filter(
      (column) => !column.hidden,
    );
    const firstOffset = columns.length > 0 ? columns[0].offsetLeft : 0;
    const normalizedTargets = columns.map((column) => Math.max(0, Math.round(column.offsetLeft - firstOffset)));
    const contentRightEdge =
      normalizedTargets.length > 0
        ? Math.max(
            ...columns.map((column, index) => normalizedTargets[index] + Math.round(column.getBoundingClientRect().width)),
          )
        : 0;
    const maxStartIndexCandidate = normalizedTargets.findIndex(
      (target) => contentRightEdge - target <= ((viewport?.clientWidth ?? 0) + 1),
    );
    const maxStartIndex =
      maxStartIndexCandidate >= 0 ? maxStartIndexCandidate : Math.max(0, normalizedTargets.length - 1);
    const logicalTargets = normalizedTargets.slice(0, maxStartIndex + 1);
    const logicalStartKeys = columns.slice(0, maxStartIndex + 1).map((column) => column.dataset.columnKey || '');
    const viewportMaxScrollLeft = Math.max(0, Math.round((viewport?.scrollWidth ?? 0) - (viewport?.clientWidth ?? 0)));
    const logicalMaxTarget =
      logicalTargets.length > 0 ? Math.min(logicalTargets[logicalTargets.length - 1], viewportMaxScrollLeft) : 0;
    const viewportRect = viewport?.getBoundingClientRect();
    const leadingVisibleColumn =
      columns.find((column) => {
        if (!viewportRect) {
          return false;
        }

        const rect = column.getBoundingClientRect();
        return rect.right > viewportRect.left + 1 && rect.left >= viewportRect.left - 4;
      }) ??
      columns.find((column) => {
        if (!viewportRect) {
          return false;
        }

        return column.getBoundingClientRect().right > viewportRect.left + 1;
      }) ??
      null;

    return {
      logicalTargets,
      logicalStartKeys,
      logicalMaxTarget,
      visibleColumnKeys: columns.map((column) => column.dataset.columnKey || ''),
      leadingVisibleColumnKey: leadingVisibleColumn?.dataset.columnKey || null,
      viewportScrollLeft: Math.round(viewport?.scrollLeft ?? 0),
      scrollbarScrollLeft: Math.round(scrollbar?.scrollLeft ?? 0),
    };
  });
}

async function waitForNmScrollTarget(page: Page, expectedScrollLeft: number): Promise<NmPathMetrics> {
  await expect
    .poll(async () => {
      const metrics = await readNmPathMetrics(page);
      return {
        viewportScrollLeft: metrics.viewportScrollLeft,
        scrollbarScrollLeft: metrics.scrollbarScrollLeft,
      };
    })
    .toEqual({
      viewportScrollLeft: expectedScrollLeft,
      scrollbarScrollLeft: expectedScrollLeft,
    });

  return readNmPathMetrics(page);
}

async function clickNmLens(page: Page, lensKey: string): Promise<void> {
  await page.locator(`[data-testid="nm-path-lens-toggle"][data-lens-key="${lensKey}"]`).click();
}

async function clickNmViewMode(page: Page, viewMode: string): Promise<void> {
  await page.locator(`[data-testid="nm-path-view-toggle"][data-view-mode="${viewMode}"]`).click();
}

async function clickNmSurfaceMode(page: Page, surfaceMode: string): Promise<void> {
  await page.locator(`[data-testid="nm-path-surface-toggle"][data-surface-mode="${surfaceMode}"]`).click();
}

async function expectNmColumnRemovedFromLayout(page: Page, columnKey: string): Promise<void> {
  const column = page.locator(`[data-testid="nm-path-column"][data-column-key="${columnKey}"]`);
  await expect(column).toBeHidden();
  await expect(column).toHaveAttribute('hidden', '');
  await expect
    .poll(async () =>
      column.evaluate((element) => window.getComputedStyle(element as HTMLElement).display),
    )
    .toBe('none');
}

async function readPreviewMetrics(page: Page, columnKey: string): Promise<NmPreviewMetrics> {
  return page.locator(`[data-testid="nm-path-column"][data-column-key="${columnKey}"]`).evaluate((column) => {
    const frame = column.querySelector('.nm-column__surface-frame') as HTMLElement | null;
    const activator = column.querySelector('[data-testid="nm-surface-open"]') as HTMLElement | null;
    const thumbnail = column.querySelector('.nm-thumbnail__device') as HTMLElement | null;
    const rendering = column.querySelector('.nm-column__surface-rendering') as HTMLElement | null;
    const columnRect = column.getBoundingClientRect();
    const frameRect = frame?.getBoundingClientRect();
    const activatorRect = activator?.getBoundingClientRect();
    const thumbnailRect = thumbnail?.getBoundingClientRect();
    const renderingRect = rendering?.getBoundingClientRect();

    return {
      columnHeight: Math.round(columnRect.height),
      frameWidth: Math.round(frameRect?.width ?? 0),
      frameHeight: Math.round(frameRect?.height ?? 0),
      activatorHeight: Math.round(activatorRect?.height ?? 0),
      thumbnailWidth: Math.round(thumbnailRect?.width ?? 0),
      thumbnailHeight: Math.round(thumbnailRect?.height ?? 0),
      renderingWidth: Math.round(renderingRect?.width ?? 0),
      renderingHeight: Math.round(renderingRect?.height ?? 0),
    };
  });
}

async function readSlotHeights(page: Page, columnKey: string): Promise<NmSlotHeights> {
  return page.locator(`[data-testid="nm-path-column"][data-column-key="${columnKey}"]`).evaluate((column) => {
    const readSlotHeight = (slotKind: string) => {
      const slot = column.querySelector(`[data-slot-kind="${slotKind}"]`) as HTMLElement | null;
      return Math.round(slot?.getBoundingClientRect().height ?? 0);
    };

    return {
      header: readSlotHeight('header'),
      screen: readSlotHeight('screen'),
      primary: readSlotHeight('primary'),
      secondary: readSlotHeight('secondary'),
      gwt: readSlotHeight('gwt'),
    };
  });
}

async function expectNmUpdateStatusToUseLocalDisplay(page: Page): Promise<void> {
  const updateStatus = page.getByTestId('nm-path-update-status');
  await expect(updateStatus).toBeVisible();

  await expect
    .poll(async () => ((await updateStatus.textContent()) || '').trim())
    .toMatch(/^(?:[A-Z ]+ \| )?UPDATED: .+ \| (?:just now|\d+ (?:minute|minutes|hour|hours|day|days) ago)$/);

  const statusText = ((await updateStatus.textContent()) || '').trim();
  expect(statusText).not.toContain('UPDATED AT:');
  expect(statusText).not.toMatch(/\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/);
  expect(statusText).not.toMatch(/\b(?:EST|EDT|CST|CDT|MST|MDT|PST|PDT|UTC|GMT)\b/);
}

function popoverForPill(pill: Locator): Locator {
  return pill
    .locator('xpath=ancestor::div[@data-testid="nm-column-pill-wrap"][1]')
    .locator('[data-testid="nm-column-pill-popover"]');
}

test.describe('PATH1 NM workspace artifact', () => {
  test('next and end navigation move by whole logical columns and keep the rail synchronized', async ({ page }) => {
    await page.goto(nmPathHttpPath);
    await expect(page.getByRole('heading', { name: 'PATH 1 NM: Fresh First Launch -> Need Location -> First Entry' })).toBeVisible();
    await expect(page.getByTestId('nm-path-app-pill')).toHaveText('LaundryLog');
    await expect(page.locator('.nm-column__app-pill')).toHaveCount(0);

    const initialMetrics = await readNmPathMetrics(page);

    expect(initialMetrics.leadingVisibleColumnKey).toBe(initialMetrics.logicalStartKeys[0]);
    expect(initialMetrics.logicalTargets.length).toBeGreaterThan(1);

    await page.getByTestId('nm-path-nav-next').click();
    const afterNext = await waitForNmScrollTarget(page, initialMetrics.logicalTargets[1]);

    expect(afterNext.leadingVisibleColumnKey).toBe(initialMetrics.logicalStartKeys[1]);

    await page.getByTestId('nm-path-nav-end').click();
    const atEnd = await waitForNmScrollTarget(page, initialMetrics.logicalMaxTarget);

    expect(atEnd.leadingVisibleColumnKey).toBe(initialMetrics.logicalStartKeys[initialMetrics.logicalStartKeys.length - 1]);
    await expect(page.getByTestId('nm-path-nav-next')).toBeDisabled();
    await expect(page.getByTestId('nm-path-nav-end')).toBeDisabled();

    await page.getByTestId('nm-path-nav-start').click();
    const backAtStart = await waitForNmScrollTarget(page, 0);

    expect(backAtStart.leadingVisibleColumnKey).toBe('01-app-started');
  });

  test('shared NM slots keep mixed columns aligned by row type', async ({ page }) => {
    await page.goto(nmPathHttpPath);
    await page.getByTestId('nm-path-nav-end').click();
    await waitForNmScrollTarget(page, (await readNmPathMetrics(page)).logicalMaxTarget);

    const keys = [
      '08-current-laundry-session-location',
      '09-entry-form-ready',
      '10-washer-draft',
      '11-log-laundry-expense',
      '12-current-laundry-session-washer',
      '13-logged-success',
    ] as const;

    const slotHeights = await Promise.all(keys.map((key) => readSlotHeights(page, key)));

    const first = slotHeights[0];

    for (const slotHeight of slotHeights.slice(1)) {
      expect(slotHeight.header).toBe(first.header);
      expect(slotHeight.screen).toBe(first.screen);
      expect(slotHeight.primary).toBe(first.primary);
      expect(slotHeight.secondary).toBe(first.secondary);
      expect(slotHeight.gwt).toBe(first.gwt);
    }

    expect(first.screen).toBeGreaterThan(0);
    expect(first.primary).toBeGreaterThan(0);
    expect(first.secondary).toBeGreaterThan(0);
    expect(first.gwt).toBeGreaterThan(0);
  });

  test('surface thumbnails stay small in thumbnail mode and full mode expands the live surface without breaking layout', async ({ page }) => {
    await page.goto(nmPathHttpPath);

    const thumbnailMetrics = await readPreviewMetrics(page, '09-entry-form-ready');

    expect(thumbnailMetrics.frameWidth).toBeGreaterThanOrEqual(176);
    expect(thumbnailMetrics.frameWidth).toBeLessThanOrEqual(190);
    expect(thumbnailMetrics.frameHeight).toBeGreaterThanOrEqual(132);
    expect(thumbnailMetrics.frameHeight).toBeLessThanOrEqual(144);
    expect(thumbnailMetrics.activatorHeight).toBe(thumbnailMetrics.frameHeight);
    expect(thumbnailMetrics.thumbnailWidth).toBeGreaterThanOrEqual(90);
    expect(thumbnailMetrics.thumbnailWidth).toBeLessThanOrEqual(102);
    expect(thumbnailMetrics.thumbnailHeight).toBeGreaterThanOrEqual(112);
    expect(thumbnailMetrics.thumbnailHeight).toBeLessThanOrEqual(154);
    expect(thumbnailMetrics.columnHeight).toBeLessThan(980);
    expect(thumbnailMetrics.renderingWidth).toBe(0);

    await expect(
      page.locator('[data-testid="nm-path-column"][data-column-key="05-need-location"] [data-testid="nm-column-screen-box"] .nm-column__detail-title'),
    ).toHaveText('Set Location Screen');
    await expect(
      page.locator('[data-testid="nm-path-column"][data-column-key="01-app-started"] [data-testid="nm-column-screen-box"] .nm-column__detail-title'),
    ).toHaveText('Splash Screen');

    await clickNmSurfaceMode(page, 'full');
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-surface-mode', 'full');

    const fullMetrics = await readPreviewMetrics(page, '09-entry-form-ready');

    expect(fullMetrics.frameWidth).toBeGreaterThan(thumbnailMetrics.frameWidth);
    expect(fullMetrics.frameHeight).toBeGreaterThan(thumbnailMetrics.frameHeight);
    expect(fullMetrics.frameHeight).toBeGreaterThanOrEqual(220);
    expect(fullMetrics.columnHeight).toBeLessThan(1120);
    expect(fullMetrics.thumbnailWidth).toBe(0);
    expect(fullMetrics.renderingWidth).toBeGreaterThan(thumbnailMetrics.renderingWidth);
    expect(fullMetrics.renderingWidth).toBeGreaterThan(90);

    await page.reload();
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-surface-mode', 'full');
  });

  test('lens and view toggles change visible content and persist across reloads', async ({ page }) => {
    await page.goto(nmPathHttpPath);

    await clickNmLens(page, 'aem');
    await expect
      .poll(async () => (await readNmPathMetrics(page)).visibleColumnKeys)
      .toEqual([
        '01-app-started',
        '02-runtime-checks',
        '03-no-local-session',
        '04-route-resolved',
        '05-need-location',
        '06-ready-to-set-location',
        '09-entry-form-ready',
        '10-washer-draft',
        '13-logged-success',
      ]);

    await expectNmColumnRemovedFromLayout(page, '07-capture-laundry-location');
    await expectNmColumnRemovedFromLayout(page, '11-log-laundry-expense');

    await clickNmLens(page, 'lifecycle');
    await expect
      .poll(async () => (await readNmPathMetrics(page)).visibleColumnKeys)
      .toEqual([
        '02-runtime-checks',
        '03-no-local-session',
        '04-route-resolved',
        '05-need-location',
        '06-ready-to-set-location',
        '09-entry-form-ready',
        '10-washer-draft',
        '13-logged-success',
      ]);

    await expectNmColumnRemovedFromLayout(page, '01-app-started');

    const runtimeColumn = page.locator('[data-testid="nm-path-column"][data-column-key="02-runtime-checks"]');
    const scenarioDisclosure = page.locator('[data-testid="nm-path-scenario"]');
    await expect(scenarioDisclosure).not.toHaveAttribute('open', '');
    await expect(page.getByTestId('nm-path-scenario-panel')).toBeHidden();

    await clickNmViewMode(page, 'summary');
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-view-mode', 'summary');
    await expect(runtimeColumn.locator('[data-testid="nm-column-meta"]')).toBeHidden();
    await expect(runtimeColumn.locator('[data-testid="nm-column-changes"] [data-testid="nm-column-detail-copy"]')).toBeHidden();
    await expect(runtimeColumn.locator('[data-testid="nm-column-screen-box"] [data-testid="nm-column-detail-copy"]')).toBeHidden();
    await expect(runtimeColumn.locator('[data-testid="nm-surface-open"]')).toBeVisible();
    await expect(scenarioDisclosure).not.toHaveAttribute('open', '');
    await expect(page.getByTestId('nm-path-scenario-panel')).toBeHidden();

    await clickNmViewMode(page, 'detailed');
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-view-mode', 'detailed');
    await expect(runtimeColumn.locator('[data-testid="nm-column-meta"]')).toBeVisible();
    await expect(runtimeColumn.locator('[data-testid="nm-column-changes"] [data-testid="nm-column-detail-copy"]')).toBeVisible();
    await expect(runtimeColumn.locator('[data-testid="nm-column-screen-box"] [data-testid="nm-column-detail-copy"]')).toBeVisible();
    await expect(scenarioDisclosure).not.toHaveAttribute('open', '');
    await expect(page.getByTestId('nm-path-scenario-panel')).toBeHidden();

    await page.locator('[data-testid="nm-path-scenario-summary"]').click();
    await expect(scenarioDisclosure).toHaveAttribute('open', '');
    await expect(page.getByTestId('nm-path-scenario-panel')).toBeVisible();

    await page.reload();
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-view-mode', 'detailed');
    await expect(page.locator('[data-testid="nm-path-lens-toggle"][data-lens-key="lifecycle"]')).not.toHaveClass(/is-active/);
    await expect(page.locator('[data-testid="nm-path-lens-toggle"][data-lens-key="aem"]')).not.toHaveClass(/is-active/);
    await expect(page.locator('[data-testid="nm-path-scenario"]')).not.toHaveAttribute('open', '');
    await expectNmColumnRemovedFromLayout(page, '01-app-started');
    await expectNmColumnRemovedFromLayout(page, '07-capture-laundry-location');
  });

  test('classification pills move above the step line, surface kinds are distinct, and contextual popovers explain the current column', async ({ page }) => {
    await page.goto(nmPathHttpPath);

    const lifecycleColumn = page.locator('[data-testid="nm-path-column"][data-column-key="01-app-started"]');
    const classificationRow = lifecycleColumn.locator('[data-testid="nm-column-meta"]');
    const stepLine = lifecycleColumn.locator('.nm-column__eyebrow');

    const classificationBox = await classificationRow.boundingBox();
    const stepBox = await stepLine.boundingBox();

    expect(classificationBox).not.toBeNull();
    expect(stepBox).not.toBeNull();
    expect((classificationBox?.y ?? 0) + (classificationBox?.height ?? 0)).toBeLessThan((stepBox?.y ?? 0) + 1);

    await expect(lifecycleColumn.getByTestId('nm-column-kind')).toHaveText('BOOT');
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="05-need-location"] [data-testid="nm-column-kind"]')).toHaveText('SCREEN');
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="07-capture-laundry-location"] [data-testid="nm-column-kind"]')).toHaveText('COMMAND');
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="08-current-laundry-session-location"] [data-testid="nm-column-kind"]')).toHaveText('VIEW');

    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="02-runtime-checks"] [data-testid="nm-column-changes"][data-detail-kind="orchestration"]')).toBeVisible();
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="05-need-location"] [data-testid="nm-column-changes"][data-detail-kind="interaction"]')).toBeVisible();
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="07-capture-laundry-location"] [data-slot-kind="primary"] .slice-block--command')).toBeVisible();
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="07-capture-laundry-location"] [data-slot-kind="secondary"] .slice-block--event')).toBeVisible();
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="08-current-laundry-session-location"] [data-slot-kind="primary"] .slice-block--view')).toBeVisible();
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="07-capture-laundry-location"]')).not.toContainText('COMMAND SLICE');
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="08-current-laundry-session-location"]')).not.toContainText('VIEW SLICE');
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="07-capture-laundry-location"] .slice-card')).toHaveCount(0);

    const aemHeaderRolePill = page.locator(
      '[data-testid="nm-path-column"][data-column-key="07-capture-laundry-location"] .nm-column__classification [data-testid="nm-column-pill"][data-pill-type="role"]',
    );
    await expect(aemHeaderRolePill).toHaveCount(0);

    const aemScreenMeta = page.locator(
      '[data-testid="nm-path-column"][data-column-key="07-capture-laundry-location"] [data-testid="nm-column-screen-meta"]',
    );
    await expect(aemScreenMeta).toBeVisible();
    await expect(aemScreenMeta.locator('[data-testid="nm-column-pill"][data-pill-type="screen-role"]')).toHaveText('User');
    await expect(aemScreenMeta.locator('[data-testid="nm-column-pill"][data-pill-type="screen-lens"]')).toHaveText('ui lens');

    const lifecycleContextPill = lifecycleColumn.locator('[data-testid="nm-column-pill"][data-pill-type="context"][data-pill-label="ApplicationLifecycle"]');
    const lifecycleContextPopover = popoverForPill(lifecycleContextPill);
    await lifecycleContextPill.hover();
    await expect(lifecycleContextPopover).toBeVisible();
    await expect(lifecycleContextPopover).toContainText('ApplicationLifecycle owns the meaning of app phase changes like start, resume, and suspend.');
    await expect(lifecycleContextPopover).toContainText('AppStarted marks an app lifecycle phase becoming visible.');

    const runtimeContextPill = page.locator('[data-testid="nm-path-column"][data-column-key="02-runtime-checks"] [data-testid="nm-column-pill"][data-pill-type="context"][data-pill-label="RuntimeOrchestration"]');
    const runtimeContextPopover = popoverForPill(runtimeContextPill);
    await runtimeContextPill.hover();
    await expect(runtimeContextPopover).toBeVisible();
    await expect(runtimeContextPopover).toContainText('RuntimeOrchestration owns startup checks, route resolution, and coordination between app/runtime and the business flow.');
    await expect(runtimeContextPopover).toContainText('Runtime Checks coordinates checks, route choice, or state handoff.');

    const userRolePill = page.locator('[data-testid="nm-path-column"][data-column-key="05-need-location"] [data-testid="nm-column-pill"][data-pill-type="role"][data-pill-label="User"]');
    const userRolePopover = popoverForPill(userRolePill);
    await userRolePill.click();
    await expect(userRolePopover).toBeVisible();
    await expect(userRolePopover).toContainText('User means the human is acting through the UI at this point in the path.');
    await expect(userRolePopover).toContainText('Need Location depends on or expresses a direct user action.');

    const aemScreenLensPill = page.locator(
      '[data-testid="nm-path-column"][data-column-key="07-capture-laundry-location"] [data-testid="nm-column-pill"][data-pill-type="screen-lens"][data-pill-label="ui lens"]',
    );
    const aemScreenLensPopover = popoverForPill(aemScreenLensPill);
    await aemScreenLensPill.click();
    await expect(aemScreenLensPopover).toBeVisible();
    await expect(aemScreenLensPopover).toContainText('ui lens marks the linked app surface that frames the business slice.');
    await expect(aemScreenLensPopover).toContainText('CaptureLaundryLocation is being grounded in the app surface around the business slice.');
  });

  test('surface thumbnails open the overlay and the tracked file artifact still loads directly from disk', async ({ page }) => {
    await page.goto(nmPathHttpPath);

    await page
      .locator('[data-testid="nm-path-column"][data-column-key="05-need-location"] [data-testid="nm-surface-open"]')
      .click();
    await expect(page.getByTestId('nm-surface-overlay')).toBeVisible();
    await expect(page.getByTestId('nm-surface-overlay').getByRole('heading', { name: 'Need Location' })).toBeVisible();
    await page.getByTestId('nm-surface-overlay-close').click();
    await expect(page.getByTestId('nm-surface-overlay')).toBeHidden();

    await page.goto(nmPathFileUrl);
    await expect(page.getByRole('heading', { name: 'PATH 1 NM: Fresh First Launch -> Need Location -> First Entry' })).toBeVisible();
    await expect(page.getByTestId('nm-path-nav-start')).toBeVisible();
    await expect(page.getByTestId('nm-path-view-toggle').first()).toBeVisible();
  });

  test('update status shows local time with a live relative-age format instead of the raw UTC timestamp', async ({ page }) => {
    await page.goto(nmPathHttpPath);
    await expectNmUpdateStatusToUseLocalDisplay(page);
  });
});
