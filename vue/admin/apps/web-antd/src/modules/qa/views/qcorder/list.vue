<script lang="ts" setup>
import type { OnActionClickParams, VxeTableGridOptions } from '#/adapter/vxe-table';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { Page, useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';
import { Button, message, Modal as AntModal } from 'ant-design-vue';

import { $t } from '#/locales';

import type { QCOrderApi } from '#/modules/qa/api/qcorder';
import {
  cancelQCOrder,
  deleteQCOrder,
  getQCOrderPage,
  judgeQCOrder,
  submitInspectQCOrder,
} from '#/modules/qa/api/qcorder';

import { useColumns } from './data';
import Form from './modules/form.vue';

const [FormModal, formModalApi] = useVbenModal({
  connectedComponent: Form,
  destroyOnClose: true,
});

function onEdit(row: QCOrderApi.QCOrderItem) {
  formModalApi.setData(row).open();
}

function onCreate() {
  formModalApi.setData(null).open();
}

function onDelete(row: QCOrderApi.QCOrderItem) {
  AntModal.confirm({
    title: $t('ui.actionMessage.deleteConfirm', [row.code]),
    content: '',
    okText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    onOk: () => {
      return deleteQCOrder(row.id!).then(() => {
        message.success($t('ui.actionMessage.deleteSuccess', [row.code]));
        refreshGrid();
      });
    },
  });
}

function getStatusActionLabel(code: string): string {
  const labels: Record<string, string> = {
    submitInspect: $t('qa.qcorder.submitInspect'),
    judgePass: $t('qa.qcorder.judgePass'),
    judgeFail: $t('qa.qcorder.judgeFail'),
    cancel: $t('qa.qcorder.cancel'),
  };
  return labels[code] || code;
}

function onStatusAction(code: string, row: QCOrderApi.QCOrderItem) {
  AntModal.confirm({
    title: `确认${getStatusActionLabel(code)}质检单 ${row.code}？`,
    content: '',
    okText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    onOk: async () => {
      try {
        switch (code) {
          case 'submitInspect': await submitInspectQCOrder({ id: row.id! }); break;
          case 'judgePass': await judgeQCOrder({ id: row.id!, conclusion: 'Pass', result: 2 }); break;
          case 'judgeFail': await judgeQCOrder({ id: row.id!, conclusion: 'Fail', result: 3 }); break;
          case 'cancel': await cancelQCOrder({ id: row.id! }); break;
        }
        message.success('操作成功');
        refreshGrid();
      } catch {
        // error handled by interceptor
      }
    },
  });
}

function getStatusActions(row: QCOrderApi.QCOrderItem): Array<{ code: string; label: string }> {
  const s = row.status;
  const actions: Array<{ code: string; label: string }> = [];
  if (s === 0) actions.push(
    { code: 'submitInspect', label: $t('qa.qcorder.submitInspect') },
  );
  if (s === 1) actions.push(
    { code: 'judgePass', label: $t('qa.qcorder.judgePass') },
    { code: 'judgeFail', label: $t('qa.qcorder.judgeFail') },
  );
  if (s === 0) actions.push({ code: 'cancel', label: $t('qa.qcorder.cancel') });
  return actions;
}

function onActionClick({ code, row }: OnActionClickParams<QCOrderApi.QCOrderItem>) {
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
          return await getQCOrderPage({
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
    <Grid :table-title="$t('qa.qcorder.list')">
      <template #toolbar-tools>
        <Button type="primary" @click="onCreate">
          <Plus class="size-5" />
          {{ $t('ui.actionTitle.create', [$t('qa.qcorder.name')]) }}
        </Button>
      </template>
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
