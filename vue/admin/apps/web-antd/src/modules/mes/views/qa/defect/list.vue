<script lang="ts" setup>
import type { OnActionClickParams, VxeTableGridOptions } from '#/adapter/vxe-table';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { Page, useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';
import { Button, message, Modal as AntModal } from 'ant-design-vue';

import { $t } from '#/locales';

import type { DefectApi } from '#/modules/mes/api/defect';
import {
  deleteDefect,
  getDefectPage,
  handleDefect,
  reviewDefect,
} from '#/modules/mes/api/defect';

import { useColumns } from './data';
import Form from './modules/form.vue';

const [FormModal, formModalApi] = useVbenModal({
  connectedComponent: Form,
  destroyOnClose: true,
});

function onEdit(row: DefectApi.DefectItem) {
  formModalApi.setData(row).open();
}

function onCreate() {
  formModalApi.setData(null).open();
}

function onDelete(row: DefectApi.DefectItem) {
  AntModal.confirm({
    title: $t('ui.actionMessage.deleteConfirm', [row.defectCode]),
    content: '',
    okText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    onOk: () => {
      return deleteDefect(row.id!).then(() => {
        message.success($t('ui.actionMessage.deleteSuccess', [row.defectCode]));
        refreshGrid();
      });
    },
  });
}

function onStatusAction(code: string, row: DefectApi.DefectItem) {
  AntModal.confirm({
    title: '确认操作',
    content: '',
    okText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    onOk: async () => {
      try {
        switch (code) {
          case 'handle':
            await handleDefect({ id: row.id!, handlingType: row.handlingType ?? 1 });
            break;
          case 'review':
            await reviewDefect({ id: row.id!, reviewResult: 'Approved' });
            break;
        }
        message.success('操作成功');
        refreshGrid();
      } catch {
        // error handled by interceptor
      }
    },
  });
}

function getStatusActions(row: DefectApi.DefectItem): Array<{ code: string; label: string }> {
  const s = row.status;
  const actions: Array<{ code: string; label: string }> = [];
  if (s === 0) actions.push({ code: 'handle', label: $t('mes.qa.defect.handle') });
  if (s === 1 && row.needReview) actions.push({ code: 'review', label: $t('mes.qa.defect.review') });
  return actions;
}

function onActionClick({ code, row }: OnActionClickParams<DefectApi.DefectItem>) {
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
          return await getDefectPage({
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
    <Grid :table-title="$t('mes.qa.defect.list')">
      <template #toolbar-tools>
        <Button type="primary" @click="onCreate">
          <Plus class="size-5" />
          {{ $t('ui.actionTitle.create', [$t('mes.qa.defect.name')]) }}
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
