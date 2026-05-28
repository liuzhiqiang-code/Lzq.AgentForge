<script lang="ts" setup>
import type { OnActionClickParams, VxeTableGridOptions } from '#/adapter/vxe-table';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { Page, useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';
import { Button, message, Modal as AntModal } from 'ant-design-vue';

import { $t } from '#/locales';

import type { WorkReportApi } from '#/modules/mes/api/workreport';
import { deleteWorkReport, getWorkReportPage } from '#/modules/mes/api/workreport';

import { useColumns } from './data';
import Form from './modules/form.vue';

const [FormModal, formModalApi] = useVbenModal({
  connectedComponent: Form,
  destroyOnClose: true,
});

function onCreate() {
  formModalApi.setData(null).open();
}

function onDelete(row: WorkReportApi.WorkReportItem) {
  AntModal.confirm({
    title: $t('ui.actionMessage.deleteConfirm', [row.id]),
    content: '',
    okText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    onOk: () => {
      return deleteWorkReport(row.id!).then(() => {
        message.success('删除成功');
        refreshGrid();
      });
    },
  });
}

function onActionClick({ code, row }: OnActionClickParams<WorkReportApi.WorkReportItem>) {
  switch (code) {
    case 'delete': onDelete(row); break;
  }
}

const [Grid, gridApi] = useVbenVxeGrid({
  gridEvents: {},
  gridOptions: {
    columns: useColumns(onActionClick),
    height: 'auto',
    keepSource: true,
    pagerConfig: { enabled: true },
    proxyConfig: {
      ajax: {
        query: async ({ page }, formValues) => {
          return await getWorkReportPage({
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
    <Grid :table-title="$t('mes.workorder.workreport.list')">
      <template #toolbar-tools>
        <Button type="primary" @click="onCreate">
          <Plus class="size-5" />
          {{ $t('ui.actionTitle.create', [$t('mes.workorder.workreport.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
