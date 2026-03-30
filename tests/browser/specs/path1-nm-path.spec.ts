import { expect, test, type Locator, type Page } from '@playwright/test';
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

const orderedColumnKeys = [
  '01-launch-app',
  '02-splash-visible',
  '03-run-startup-checks',
  '04-runtime-checks-view',
  '05-inspect-local-session',
  '06-no-local-session-view',
  '07-resolve-initial-route',
  '08-need-location',
  '09-enter-location-text',
  '10-ready-to-set-location',
  '11-capture-laundry-location',
  '12-current-laundry-session-location',
  '13-entry-form-ready',
  '14-select-washer',
  '15-washer-draft',
  '16-log-laundry-expense',
  '17-current-laundry-session-washer',
  '18-logged-success',
] as const;

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
      logicalStartKeys: columns.slice(0, maxStartIndex + 1).map((column) => column.dataset.columnKey || ''),
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
    .poll(async () => column.evaluate((element) => window.getComputedStyle(element as HTMLElement).display))
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

async function readSlotContentHeight(page: Page, columnKey: string, slotKind: string): Promise<number> {
  return page.locator(`[data-testid="nm-path-column"][data-column-key="${columnKey}"]`).evaluate(
    (column, targetSlotKind) => {
      const slotBody = column.querySelector(`[data-slot-kind="${targetSlotKind}"] [data-slot-body]`) as HTMLElement | null;
      const slotContent = slotBody?.firstElementChild as HTMLElement | null;
      return Math.round(slotContent?.getBoundingClientRect().height ?? 0);
    },
    slotKind,
  );
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

function popoverForNmKind(trigger: Locator): Locator {
  return trigger
    .locator('xpath=ancestor::div[@data-testid="nm-column-kind-wrap"][1]')
    .locator('[data-testid$="popover"]');
}

function popoverForSliceHelp(trigger: Locator): Locator {
  return trigger
    .locator('xpath=ancestor::*[@data-slice-help-wrap][1]')
    .locator('[data-slice-help-popover]');
}

test.describe('PATH1 NM workspace artifact', () => {
  test('next and end navigation move by whole logical columns and keep the rail synchronized', async ({ page }) => {
    await page.goto(nmPathHttpPath);
    await expect(page.getByRole('heading', { name: 'PATH 1 NM: Fresh First Launch -> Need Location -> First Entry' })).toBeVisible();
    await expect(page.getByTestId('nm-path-app-pill')).toHaveText('LaundryLog');

    const initialMetrics = await readNmPathMetrics(page);

    expect(initialMetrics.visibleColumnKeys).toEqual([...orderedColumnKeys]);
    expect(initialMetrics.leadingVisibleColumnKey).toBe('01-launch-app');
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
    expect(backAtStart.leadingVisibleColumnKey).toBe('01-launch-app');
  });

  test('shared NM rows stay aligned across command and view slices', async ({ page }) => {
    await page.goto(nmPathHttpPath);
    await page.getByTestId('nm-path-nav-end').click();
    await waitForNmScrollTarget(page, (await readNmPathMetrics(page)).logicalMaxTarget);

    const keys = [
      '11-capture-laundry-location',
      '12-current-laundry-session-location',
      '13-entry-form-ready',
      '16-log-laundry-expense',
      '17-current-laundry-session-washer',
      '18-logged-success',
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

    const screenBoxHeights = await Promise.all(keys.map((key) => readSlotContentHeight(page, key, 'screen')));
    const gwtBoxHeights = await Promise.all(keys.map((key) => readSlotContentHeight(page, key, 'gwt')));

    expect(new Set(screenBoxHeights).size).toBe(1);
    expect(new Set(gwtBoxHeights).size).toBe(1);
  });

  test('surface thumbnails stay small in thumbnail mode and full mode expands the live surface without breaking layout', async ({ page }) => {
    await page.goto(nmPathHttpPath);

    const thumbnailMetrics = await readPreviewMetrics(page, '13-entry-form-ready');

    expect(thumbnailMetrics.frameWidth).toBeGreaterThanOrEqual(176);
    expect(thumbnailMetrics.frameWidth).toBeLessThanOrEqual(190);
    expect(thumbnailMetrics.frameHeight).toBeGreaterThanOrEqual(132);
    expect(thumbnailMetrics.frameHeight).toBeLessThanOrEqual(144);
    expect(thumbnailMetrics.activatorHeight).toBeGreaterThanOrEqual(thumbnailMetrics.frameHeight);
    expect(thumbnailMetrics.thumbnailWidth).toBeGreaterThanOrEqual(90);
    expect(thumbnailMetrics.thumbnailWidth).toBeLessThanOrEqual(102);
    expect(thumbnailMetrics.thumbnailHeight).toBeGreaterThanOrEqual(112);
    expect(thumbnailMetrics.thumbnailHeight).toBeLessThanOrEqual(154);
    expect(thumbnailMetrics.renderingWidth).toBe(0);

    await expect(
      page.locator('[data-testid="nm-path-column"][data-column-key="08-need-location"] [data-testid="nm-column-attachment-box"] .nm-column__detail-title'),
    ).toHaveText('Set Location Screen');
    await expect(
      page.locator('[data-testid="nm-path-column"][data-column-key="01-launch-app"] [data-testid="nm-column-attachment-box"] .nm-column__detail-title'),
    ).toHaveText('App Launch Trigger');

    await clickNmSurfaceMode(page, 'full');
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-surface-mode', 'full');

    const fullMetrics = await readPreviewMetrics(page, '13-entry-form-ready');

    expect(fullMetrics.frameWidth).toBeGreaterThan(thumbnailMetrics.frameWidth);
    expect(fullMetrics.frameHeight).toBeGreaterThan(thumbnailMetrics.frameHeight);
    expect(fullMetrics.renderingWidth).toBeGreaterThan(90);
    expect(fullMetrics.thumbnailWidth).toBe(0);

    await page.reload();
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-surface-mode', 'full');
  });

  test('lens and view toggles change visible NM content and persist across reloads', async ({ page }) => {
    await page.goto(nmPathHttpPath);

    const scenarioDisclosure = page.locator('[data-testid="nm-path-scenario"]');
    await expect(scenarioDisclosure).not.toHaveAttribute('open', '');
    await expect(page.getByTestId('nm-path-scenario-panel')).toBeHidden();

    await clickNmLens(page, 'aem');
    await expect
      .poll(async () => (await readNmPathMetrics(page)).visibleColumnKeys)
      .toEqual([
        '01-launch-app',
        '02-splash-visible',
        '03-run-startup-checks',
        '04-runtime-checks-view',
        '05-inspect-local-session',
        '06-no-local-session-view',
        '07-resolve-initial-route',
        '08-need-location',
        '09-enter-location-text',
        '10-ready-to-set-location',
        '13-entry-form-ready',
        '14-select-washer',
        '15-washer-draft',
        '18-logged-success',
      ]);

    await expectNmColumnRemovedFromLayout(page, '11-capture-laundry-location');
    await expectNmColumnRemovedFromLayout(page, '12-current-laundry-session-location');
    await expectNmColumnRemovedFromLayout(page, '16-log-laundry-expense');
    await expectNmColumnRemovedFromLayout(page, '17-current-laundry-session-washer');

    await clickNmLens(page, 'lifecycle');
    await expect
      .poll(async () => (await readNmPathMetrics(page)).visibleColumnKeys)
      .toEqual([
        '03-run-startup-checks',
        '04-runtime-checks-view',
        '05-inspect-local-session',
        '06-no-local-session-view',
        '07-resolve-initial-route',
        '08-need-location',
        '09-enter-location-text',
        '10-ready-to-set-location',
        '13-entry-form-ready',
        '14-select-washer',
        '15-washer-draft',
        '18-logged-success',
      ]);

    await expectNmColumnRemovedFromLayout(page, '01-launch-app');
    await expectNmColumnRemovedFromLayout(page, '02-splash-visible');

    const runtimeColumn = page.locator('[data-testid="nm-path-column"][data-column-key="03-run-startup-checks"]');

    await clickNmViewMode(page, 'summary');
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-view-mode', 'summary');
    await expect(runtimeColumn.locator('[data-testid="nm-column-meta"]')).toBeHidden();
    await expect(runtimeColumn.locator('[data-testid="nm-column-detail-copy"]')).toBeHidden();
    await expect(runtimeColumn.locator('[data-testid="nm-column-attachment-box"]')).toBeVisible();
    await expect(runtimeColumn.locator('[data-testid="nm-attachment-schematic"]')).toBeVisible();
    await expect(page.getByTestId('nm-path-scenario-panel')).toBeHidden();

    await clickNmViewMode(page, 'detailed');
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-view-mode', 'detailed');
    await expect(runtimeColumn.locator('[data-testid="nm-column-meta"]')).toBeVisible();
    await expect(runtimeColumn.locator('[data-testid="nm-column-detail-copy"]')).toBeVisible();
    await expect(page.getByTestId('nm-path-scenario-panel')).toBeHidden();

    await page.locator('[data-testid="nm-path-scenario-summary"]').click();
    await expect(scenarioDisclosure).toHaveAttribute('open', '');
    await expect(page.getByTestId('nm-path-scenario-panel')).toBeVisible();

    await page.reload();
    await expect(page.getByTestId('nm-path-document')).toHaveAttribute('data-view-mode', 'detailed');
    await expect(page.locator('[data-testid="nm-path-lens-toggle"][data-lens-key="lifecycle"]')).not.toHaveClass(/is-active/);
    await expect(page.locator('[data-testid="nm-path-lens-toggle"][data-lens-key="aem"]')).not.toHaveClass(/is-active/);
    await expect(page.locator('[data-testid="nm-path-scenario"]')).not.toHaveAttribute('open', '');
  });

  test('classification, attachment, actor, and compartment help explain the NM slice law', async ({ page }) => {
    await page.goto(nmPathHttpPath);

    const firstCommandColumn = page.locator('[data-testid="nm-path-column"][data-column-key="01-launch-app"]');
    const classificationRow = firstCommandColumn.locator('[data-testid="nm-column-meta"]');
    const stepLine = firstCommandColumn.locator('.nm-column__eyebrow');
    const classificationBox = await classificationRow.boundingBox();
    const stepBox = await stepLine.boundingBox();

    expect(classificationBox).not.toBeNull();
    expect(stepBox).not.toBeNull();
    expect((classificationBox?.y ?? 0) + (classificationBox?.height ?? 0)).toBeLessThan((stepBox?.y ?? 0) + 1);

    await expect(firstCommandColumn.getByTestId('nm-column-kind')).toHaveText('COMMAND');
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="02-splash-visible"] [data-testid="nm-column-kind"]')).toHaveText('VIEW');
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="01-launch-app"] [data-testid="nm-column-detail-kind"]')).toHaveText('TRIGGER');
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="03-run-startup-checks"] [data-testid="nm-column-detail-kind"]')).toHaveText('RUNTIME');
    await expect(page.locator('[data-testid="nm-path-column"][data-column-key="08-need-location"] [data-testid="nm-column-detail-kind"]')).toHaveText('SCREEN');

    const actorLabelRow = page.locator(
      '[data-testid="nm-path-column"][data-column-key="08-need-location"] [data-testid="nm-column-attachment-label-row"]',
    );
    const actorPill = actorLabelRow.locator('[data-testid="nm-column-pill"][data-pill-type="screen-role"]');
    const attachmentTrigger = actorLabelRow.getByTestId('nm-column-detail-kind');
    await expect(actorPill).toHaveText('User');
    const actorBox = await actorPill.boundingBox();
    const attachmentBox = await attachmentTrigger.boundingBox();
    expect(actorBox).not.toBeNull();
    expect(attachmentBox).not.toBeNull();
    expect((actorBox?.x ?? 0) + (actorBox?.width ?? 0)).toBeLessThan((attachmentBox?.x ?? 0) + 1);

    const contextPill = firstCommandColumn.locator(
      '[data-testid="nm-column-pill"][data-pill-type="context"][data-pill-label="ApplicationLifecycle"]',
    );
    const contextPopover = popoverForPill(contextPill);
    await contextPill.hover();
    await expect(contextPopover).toBeVisible();
    await expect(contextPopover).toContainText('Bounded Context');
    await expect(contextPopover).toContainText('ApplicationLifecycle owns the meaning of app phase changes like start, resume, and suspend.');
    await expect(contextPopover).toContainText('LaunchApp belongs to the app\'s launch/lifecycle story.');

    const columnKindTrigger = firstCommandColumn.getByTestId('nm-column-kind');
    const columnKindPopover = popoverForNmKind(columnKindTrigger);
    await columnKindTrigger.click();
    await expect(columnKindPopover).toBeVisible();
    await expect(columnKindPopover).toContainText('Column Classification');
    await expect(columnKindPopover).toContainText('COMMAND');
    await expect(columnKindPopover).toContainText('this whole NM column is a change slice');
    await expect(columnKindPopover).toContainText('LaunchApp models a change that culminates in an event');
    await columnKindPopover.getByTestId('nm-column-pill-popover-close').click();
    await expect(columnKindPopover).toBeHidden();

    const actorPopover = popoverForPill(actorPill);
    await actorPill.click();
    await expect(actorPopover).toBeVisible();
    await expect(actorPopover).toContainText('Actor');
    await expect(actorPopover).toContainText('Actor · User means the human is acting through the UI at this point in the path.');
    await page.mouse.move(2, 2);
    await expect(actorPopover).toBeVisible();
    await actorPopover.getByTestId('nm-column-pill-popover-close').click();
    await expect(actorPopover).toBeHidden();

    const attachmentPopover = popoverForNmKind(attachmentTrigger);
    await attachmentTrigger.click();
    await expect(attachmentPopover).toBeVisible();
    await expect(attachmentPopover).toContainText('Attachment Classification');
    await expect(attachmentPopover).toContainText('SCREEN');
    await expect(attachmentPopover).toContainText('marks the linked app surface that frames or lands a slice');
    await attachmentPopover.getByTestId('nm-column-pill-popover-close').click();
    await expect(attachmentPopover).toBeHidden();

    const commandBlockKindTrigger = page
      .locator('[data-testid="nm-path-column"][data-column-key="11-capture-laundry-location"] [data-slot-kind="primary"] [data-testid="slice-kind-trigger"]')
      .first();
    const commandBlockKindPopover = popoverForSliceHelp(commandBlockKindTrigger);
    await commandBlockKindTrigger.click();
    await expect(commandBlockKindPopover).toBeVisible();
    await expect(commandBlockKindPopover).toContainText('Compartment Classification');
    await expect(commandBlockKindPopover).toContainText('COMMAND');
    await commandBlockKindPopover.getByTestId('slice-help-popover-close').click();
    await expect(commandBlockKindPopover).toBeHidden();

    const gwtKindTrigger = page
      .locator('[data-testid="nm-path-column"][data-column-key="11-capture-laundry-location"] [data-testid="slice-gwt-kind-trigger"]')
      .first();
    const gwtKindPopover = popoverForSliceHelp(gwtKindTrigger);
    await gwtKindTrigger.click();
    await expect(gwtKindPopover).toBeVisible();
    await expect(gwtKindPopover).toContainText('Compartment Classification');
    await expect(gwtKindPopover).toContainText('COMMAND GWT');
    await gwtKindPopover.getByTestId('slice-help-popover-close').click();
    await expect(gwtKindPopover).toBeHidden();
  });

  test('universal COMMAND and VIEW slices keep EVENT as the command backbone and allow view fanout', async ({ page }) => {
    await page.goto(nmPathHttpPath);

    const launchCommand = page.locator('[data-testid="nm-path-column"][data-column-key="01-launch-app"]');
    await expect(launchCommand.getByTestId('nm-column-kind')).toHaveText('COMMAND');
    await expect(launchCommand.locator('[data-slot-kind="primary"] .slice-block--command')).toBeVisible();
    await expect(launchCommand.locator('[data-slot-kind="secondary"] .slice-block--event')).toBeVisible();
    await expect(launchCommand.locator('[data-testid="slice-gwt-kind-trigger"]')).toContainText('COMMAND GWT');
    await expect(launchCommand).toContainText('AppStarted');

    const splashView = page.locator('[data-testid="nm-path-column"][data-column-key="02-splash-visible"]');
    await expect(splashView.getByTestId('nm-column-kind')).toHaveText('VIEW');
    await expect(splashView.locator('[data-slot-kind="primary"] .slice-block--view')).toBeVisible();
    await expect(splashView.locator('[data-slot-kind="secondary"] .slice-block--event')).toHaveCount(0);
    await expect(splashView.locator('[data-testid="slice-gwt-kind-trigger"]')).toContainText('VIEW GWT');
    await expect(splashView).toContainText('AppStarted');

    const currentSessionLocationView = page.locator('[data-testid="nm-path-column"][data-column-key="12-current-laundry-session-location"]');
    const entryFormReadyView = page.locator('[data-testid="nm-path-column"][data-column-key="13-entry-form-ready"]');
    const currentSessionWasherView = page.locator('[data-testid="nm-path-column"][data-column-key="17-current-laundry-session-washer"]');
    const loggedSuccessView = page.locator('[data-testid="nm-path-column"][data-column-key="18-logged-success"]');

    await expect(currentSessionLocationView).toContainText('LaundryLocationCaptured');
    await expect(entryFormReadyView).toContainText('LaundryLocationCaptured');
    await expect(currentSessionWasherView).toContainText('LaundryExpenseLogged');
    await expect(loggedSuccessView).toContainText('LaundryExpenseLogged');
  });

  test('surface thumbnails open the overlay and the tracked file artifact still loads directly from disk', async ({ page }) => {
    await page.goto(nmPathHttpPath);

    await page
      .locator('[data-testid="nm-path-column"][data-column-key="08-need-location"] [data-testid="nm-surface-open"]')
      .click();
    await expect(page.getByTestId('nm-surface-overlay')).toBeVisible();
    await expect(page.getByTestId('nm-surface-overlay').getByRole('heading', { name: 'NeedLocation' })).toBeVisible();
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
