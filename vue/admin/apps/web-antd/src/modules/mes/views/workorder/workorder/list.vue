<script lang="ts" setup>
import type { OnActionClickParams, VxeTableGridOptions } from '#/adapter/vxe-table';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { Page, useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';
import { Button, message, Modal as AntModal } from 'ant-design-vue';
import { h } from 'vue';

import { $t } from '#/locales';

import type { WorkOrderApi } from '#/modules/mes/api/workorder';
import {
  cancelWorkOrder,
  closeWorkOrder,
  completeWorkOrder,
  deleteWorkOrder,
  dispatchWorkOrder,
  getWorkOrderPage,
  pauseWorkOrder,
  startWorkOrder,
} from '#/modules/mes/api/workorder';

import { useColumns } from './data';
import Form from './modules/form.vue';

const [FormModal, formModalApi] = useVbenModal({
  connectedComponent: Form,
  destroyOnClose: true,
});

function onEdit(row: WorkOrderApi.WorkOrderItem) {
  formModalApi.setData(row).open();
}

function onCreate() {
  formModalApi.setData(null).open();
}

function onDelete(row: WorkOrderApi.WorkOrderItem) {
  AntModal.confirm({
    title: $t('ui.actionMessage.deleteConfirm', [row.code]),
    content: '',
    okText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    onOk: () => {
      return deleteWorkOrder(row.id!).then(() => {
        message.success($t('ui.actionMessage.deleteSuccess', [row.code]));
        refreshGrid();
      });
    },
  });
}

/**
 * 状态操作(派发/开工/完工/关闭/取消/暂停)
 */
function onStatusAction(code: string, row: WorkOrderApi.WorkOrderItem) {
  AntModal.confirm({
    title: `确认${getStatusActionLabel(code)}工单 ${row.code}？`,
    content: '',
    okText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    onOk: async () => {
      try {
        switch (code) {
          case 'dispatch': await dispatchWorkOrder({ id: row.id! }); break;
          case 'start': await startWorkOrder({ id: row.id! }); break;
          case 'complete': await completeWorkOrder({ id: row.id! }); break;
          case 'close': await closeWorkOrder({ id: row.id! }); break;
          case 'cancel': await cancelWorkOrder({ id: row.id! }); break;
          case 'pause': await pauseWorkOrder({ id: row.id! }); break;
        }
        message.success('操作成功');
        refreshGrid();
      } catch {
        // error handled by interceptor
      }
    },
  });
}

function getStatusActionLabel(code: string): string {
  const labels: Record<string, string> = {
    dispatch: $t('mes.workorder.workorder.dispatch'),
    start: $t('mes.workorder.workorder.start'),
    complete: $t('mes.workorder.workorder.complete'),
    close: $t('mes.workorder.workorder.close'),
    cancel: $t('mes.workorder.workorder.cancel'),
    pause: $t('mes.workorder.workorder.pause'),
  };
  return labels[code] || code;
}

/**
 * 根据工单状态生成可用的操作按钮
 */
function getStatusActions(row: WorkOrderApi.WorkOrderItem): Array<{ code: string; label: string }> {
  const s = row.status;
  const actions: Array<{ code: string; label: string }> = [];
  if (s === 0) actions.push({ code: 'dispatch', label: $t('mes.workorder.workorder.dispatch') });
  if (s === 1) actions.push({ code: 'start', label: $t('mes.workorder.workorder.start') }, { code: 'cancel', label: $t('mes.workorder.workorder.cancel') });
  if (s === 2) actions.push({ code: 'complete', label: $t('mes.workorder.workorder.complete') }, { code: 'pause', label: $t('mes.workorder.workorder.pause') });
  if (s === 6) actions.push({ code: 'start', label: $t('mes.workorder.workorder.resume') });
  if (s === 3) actions.push({ code: 'close', label: $t('mes.workorder.workorder.close') });
  return actions;
}

function onActionClick({ code, row }: OnActionClickParams<WorkOrderApi.WorkOrderItem>) {
  switch (code) {
    case 'delete': onDelete(row); break;
    case 'edit': onEdit(row); break;
  }
}

const [Grid, gridApi] = useVbenVxeGrid({
  gridEvents: {},
  gridOptions: {
    columns: useColumns(onActionClick, onStatusAction),
    height: 'auto',
    keepSource: true,
    pagerConfig: { enabled: true },
    proxyConfig: {
      ajax: {
        query: async ({ page }, formValues) => {
          return await getWorkOrderPage({
            page: page.currentPage,
            pageSize: page.pageSize,
            ...formValues,
          });
        },
      },
    },
    toolbarConfig: { custom: true, export: false, refresh: true, zoom: true },
  } as VxeTableGridOptions,
});

function refreshGrid() {
  gridApi.query();
}
</script>
<template>
  <Page auto-content-height>
    <FormModal @success="refreshGrid" />
    <Grid :table-title="$t('mes.workorder.workorder.list')">
      <template #toolbar-tools>
        <Button type="primary" @click="onCreate">
          <Plus class="size-5" />
          {{ $t('ui.actionTitle.create', [$t('mes.workorder.workorder.name')]) }}
        </Button>
      </template>
      <!-- 状态操作列 -->
      <template #statusAction="{ row }">
        <template v-for="action in getStatusActions(row)" :key="action.code">
          <Button size="small" type="link" @click="onStatusAction(action.code, row)">
            {{ action.label }}
          </Button>
        </template>
      </template>
    </Grid>
  </Page>
</template>
