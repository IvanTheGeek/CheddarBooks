import { expect, test, type Page } from '@playwright/test';
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
  logicalMaxTarget: number;
  visibleColumnKeys: string[];
  leadingVisibleColumnKey: string | null;
  viewportScrollLeft: number;
  scrollbarScrollLeft: number;
};

type NmPreviewMetrics = {
  columnHeight: number;
  frameHeight: number;
  activatorHeight: number;
  renderingWidth: number;
  renderingHeight: number;
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
    const logicalMaxTarget = logicalTargets.length > 0 ? logicalTargets[logicalTargets.length - 1] : 0;
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
    const rendering = column.querySelector('.nm-column__surface-rendering') as HTMLElement | null;
    const columnRect = column.getBoundingClientRect();
    const frameRect = frame?.getBoundingClientRect();
    const activatorRect = activator?.getBoundingClientRect();
    const renderingRect = rendering?.getBoundingClientRect();

    return {
      columnHeight: Math.round(columnRect.height),
      frameHeight: Math.round(frameRect?.height ?? 0),
      activatorHeight: Math.round(activatorRect?.height ?? 0),
      renderingWidth: Math.round(renderingRect?.width ?? 0),
      renderingHeight: Math.round(renderingRect?.height ?? 0),
    };
  });
}

async function expectNmUpdateStatusToUseLocalDisplay(page: Page): Promise<void> {
  const updateStatus = page.getByTestId('nm-path-update-status');
  await expect(updateStatus).toBeVisible();

  await expect
    .poll(async () => ((await updateStatus.textContent()) || '').trim())
    .toMatch(/^(?:[A-Z ]+ \| )?UPDATED AT: .+ \| (?:just now|\d+ (?:minute|minutes|hour|hours|day|days) ago)$/);

  const statusText = ((await updateStatus.textContent()) || '').trim();
  expect(statusText).not.toMatch(/\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/);
}

test.describe('PATH1 NM workspace artifact', () => {
  test('next and end navigation move by whole logical columns and keep the rail synchronized', async ({ page }) => {
    await page.goto(nmPathHttpPath);
    await expect(page.getByRole('heading', { name: 'PATH 1 NM: Fresh First Launch -> Need Location -> First Entry' })).toBeVisible();
    await expect(page.getByTestId('nm-path-app-pill')).toHaveText('LaundryLog');
    await expect(page.locator('.nm-column__app-pill')).toHaveCount(0);

    const initialMetrics = await readNmPathMetrics(page);

    expect(initialMetrics.leadingVisibleColumnKey).toBe('01-app-started');
    expect(initialMetrics.logicalTargets.length).toBeGreaterThan(1);

    await page.getByTestId('nm-path-nav-next').click();
    const afterNext = await waitForNmScrollTarget(page, initialMetrics.logicalTargets[1]);

    expect(afterNext.leadingVisibleColumnKey).toBe('02-runtime-checks');

    await page.getByTestId('nm-path-nav-end').click();
    const atEnd = await waitForNmScrollTarget(page, initialMetrics.logicalMaxTarget);

    expect(atEnd.leadingVisibleColumnKey).toBe('10-washer-draft');
    await expect(page.getByTestId('nm-path-nav-next')).toBeDisabled();
    await expect(page.getByTestId('nm-path-nav-end')).toBeDisabled();

    await page.getByTestId('nm-path-nav-start').click();
    const backAtStart = await waitForNmScrollTarget(page, 0);

    expect(backAtStart.leadingVisibleColumnKey).toBe('01-app-started');
  });

  test('surface previews stay clipped in thumbnail mode and full mode expands them without breaking layout', async ({ page }) => {
    await page.goto(nmPathHttpPath);

    const thumbnailMetrics = await readPreviewMetrics(page, '09-entry-form-ready');

    expect(thumbnailMetrics.frameHeight).toBeGreaterThanOrEqual(220);
    expect(thumbnailMetrics.frameHeight).toBeLessThanOrEqual(236);
    expect(thumbnailMetrics.activatorHeight).toBe(thumbnailMetrics.frameHeight);
    expect(thumbnailMetrics.columnHeight).toBeLessThan(560);
    expect(thumbnailMetrics.renderingWidth).toBeLessThan(220);

    await clickNmSurfaceMode(page, 'full');
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-surface-mode', 'full');

    const fullMetrics = await readPreviewMetrics(page, '09-entry-form-ready');

    expect(fullMetrics.frameHeight).toBeGreaterThan(thumbnailMetrics.frameHeight);
    expect(fullMetrics.frameHeight).toBeGreaterThanOrEqual(312);
    expect(fullMetrics.columnHeight).toBeLessThan(700);

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
    await clickNmViewMode(page, 'summary');
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-view-mode', 'summary');
    await expect(runtimeColumn.locator('[data-testid="nm-column-meta"]')).toBeHidden();
    await expect(runtimeColumn.locator('[data-testid="nm-column-changes"]')).toBeHidden();
    await expect(page.getByTestId('nm-path-scenario-panel')).toBeHidden();

    await page.locator('[data-testid="nm-path-scenario-summary"]').click();
    await expect(scenarioDisclosure).not.toHaveAttribute('open', '');

    await clickNmViewMode(page, 'detailed');
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-view-mode', 'detailed');
    await expect(runtimeColumn.locator('[data-testid="nm-column-meta"]')).toBeVisible();
    await expect(runtimeColumn.locator('[data-testid="nm-column-changes"]')).toBeVisible();
    await expect(scenarioDisclosure).not.toHaveAttribute('open', '');
    await expect(page.getByTestId('nm-path-scenario-panel')).toBeHidden();

    await page.locator('[data-testid="nm-path-scenario-summary"]').click();
    await expect(scenarioDisclosure).toHaveAttribute('open', '');
    await expect(page.getByTestId('nm-path-scenario-panel')).toBeVisible();

    await page.reload();
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-view-mode', 'detailed');
    await expect(page.locator('[data-testid="nm-path-lens-toggle"][data-lens-key="lifecycle"]')).not.toHaveClass(/is-active/);
    await expect(page.locator('[data-testid="nm-path-lens-toggle"][data-lens-key="aem"]')).not.toHaveClass(/is-active/);
    await expectNmColumnRemovedFromLayout(page, '01-app-started');
    await expectNmColumnRemovedFromLayout(page, '07-capture-laundry-location');
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
