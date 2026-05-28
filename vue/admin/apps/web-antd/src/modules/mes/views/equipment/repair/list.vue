<script lang="ts" setup>
import type { OnActionClickParams, VxeTableGridOptions } from '#/adapter/vxe-table';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { Page, useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';
import { Button, message, Modal as AntModal } from 'ant-design-vue';

import { $t } from '#/locales';

import type { RepairApi } from '#/modules/mes/api/repair';
import {
  acceptRepair,
  cancelRepair,
  getRepairPage,
  startRepair,
} from '#/modules/mes/api/repair';

import { useColumns } from './data';
import Form from './modules/form.vue';
import AssignForm from './modules/assign-form.vue';
import CompleteForm from './modules/complete-form.vue';

// ====== Modals ======
const [FormModal, formModalApi] = useVbenModal({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [AssignModal, assignModalApi] = useVbenModal({
  connectedComponent: AssignForm,
  destroyOnClose: true,
});

const [CompleteModal, completeModalApi] = useVbenModal({
  connectedComponent: CompleteForm,
  destroyOnClose: true,
});

// ====== Actions ======
function onCreate() {
  formModalApi.setData(null).open();
}

function onAssign(row: RepairApi.RepairItem) {
  assignModalApi.setData(row).open();
}

function onStart(row: RepairApi.RepairItem) {
  AntModal.confirm({
    title: $t('ui.actionMessage.confirmTitle', [$t('mes.equipment.repair.start'), row.code || '']),
    content: '',
    okText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    onOk: () => {
      return startRepair({ id: row.id! }).then(() => {
        message.success($t('ui.actionMessage.operationSuccess'));
        refreshGrid();
      });
    },
  });
}

function onComplete(row: RepairApi.RepairItem) {
  completeModalApi.setData(row).open();
}

function onAccept(row: RepairApi.RepairItem) {
  AntModal.confirm({
    title: $t('ui.actionMessage.confirmTitle', [$t('mes.equipment.repair.accept'), row.code || '']),
    content: '',
    okText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    onOk: () => {
      return acceptRepair({ id: row.id! }).then(() => {
        message.success($t('ui.actionMessage.operationSuccess'));
        refreshGrid();
      });
    },
  });
}

function onCancel(row: RepairApi.RepairItem) {
  AntModal.confirm({
    title: $t('ui.actionMessage.confirmTitle', [$t('mes.equipment.repair.cancel'), row.code || '']),
    content: '',
    okText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    okType: 'danger',
    onOk: () => {
      return cancelRepair(row.id!).then(() => {
        message.success($t('ui.actionMessage.operationSuccess'));
        refreshGrid();
      });
    },
  });
}

function onActionClick({ code, row }: OnActionClickParams<RepairApi.RepairItem>) {
  switch (code) {
    case 'assign':
      onAssign(row);
      break;
    case 'start':
      onStart(row);
      break;
    case 'complete':
      onComplete(row);
      break;
    case 'accept':
      onAccept(row);
      break;
    case 'cancel':
      onCancel(row);
      break;
  }
}

// ====== Grid ======
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
          return await getRepairPage({
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
    <AssignModal @success="refreshGrid" />
    <CompleteModal @success="refreshGrid" />
    <Grid :table-title="$t('mes.equipment.repair.list')">
      <template #toolbar-tools>
        <Button type="primary" @click="onCreate">
          <Plus class="size-5" />
          {{ $t('ui.actionTitle.create', [$t('mes.equipment.repair.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
