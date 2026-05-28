<script lang="ts" setup>
import type { OnActionClickParams, VxeTableGridOptions } from '#/adapter/vxe-table';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { Page, useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';
import { Button, message, Modal as AntModal } from 'ant-design-vue';

import { $t } from '#/locales';

import type { InspectionPlanApi } from '#/modules/mes/api/inspection';
import { deleteInspectionPlan, getInspectionPlanPage } from '#/modules/mes/api/inspection';

import { useColumns } from './data';
import Form from './modules/form.vue';

const [FormModal, formModalApi] = useVbenModal({
  connectedComponent: Form,
  destroyOnClose: true,
});

function onEdit(row: InspectionPlanApi.InspectionPlanItem) {
  formModalApi.setData(row).open();
}

function onCreate() {
  formModalApi.setData(null).open();
}

function onDelete(row: InspectionPlanApi.InspectionPlanItem) {
  AntModal.confirm({
    title: $t('ui.actionMessage.deleteConfirm', [row.name]),
    content: '',
    okText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    onOk: () => {
      return deleteInspectionPlan(row.id!).then(() => {
        message.success($t('ui.actionMessage.deleteSuccess', [row.name]));
        refreshGrid();
      });
    },
  });
}

function onActionClick({ code, row }: OnActionClickParams<InspectionPlanApi.InspectionPlanItem>) {
  switch (code) {
    case 'delete': onDelete(row); break;
    case 'edit': onEdit(row); break;
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
          return await getInspectionPlanPage({
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
    <Grid :table-title="$t('mes.equipment.inspectionPlan.list')">
      <template #toolbar-tools>
        <Button type="primary" @click="onCreate">
          <Plus class="size-5" />
          {{ $t('ui.actionTitle.create', [$t('mes.equipment.inspectionPlan.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
