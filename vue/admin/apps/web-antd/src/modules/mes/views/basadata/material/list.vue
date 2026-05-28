<script lang="ts" setup>
import { ref } from 'vue';
import type { OnActionClickParams, VxeTableGridOptions } from '#/adapter/vxe-table';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { Page, useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';
import { Button, message, Modal as AntModal } from 'ant-design-vue';

import { $t } from '#/locales';

import type { MaterialApi } from '#/modules/mes/api/material';
import { deleteMaterial, getMaterialPage } from '#/modules/mes/api/material';

import { useColumns, useSearchSchema } from './data';
import Form from './modules/form.vue';
import BomPanel from './modules/bom-panel.vue';

const selectedProduct = ref<{ id?: number; name?: string }>();

const [FormModal, formModalApi] = useVbenModal({
  connectedComponent: Form,
  destroyOnClose: true,
});

function onEdit(row: MaterialApi.MaterialItem) {
  formModalApi.setData(row).open();
}

function onCreate() {
  formModalApi.setData(null).open();
}

function onDelete(row: MaterialApi.MaterialItem) {
  AntModal.confirm({
    title: $t('ui.actionMessage.deleteConfirm', [row.name]),
    content: '',
    okText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    onOk: async () => {
      try {
        await deleteMaterial(row.id!);
        message.success($t('ui.actionMessage.deleteSuccess', [row.name]));
        refreshGrid();
        if (selectedProduct.value?.id === row.id) {
          selectedProduct.value = undefined;
        }
      } catch (e: any) {
        message.error(e?.message || $t('ui.actionMessage.deleteFailed'));
      }
    },
  });
}

function onBomManage(row: MaterialApi.MaterialItem) {
  selectedProduct.value = { id: row.id, name: row.name };
}

function onActionClick({ code, row }: OnActionClickParams<MaterialApi.MaterialItem>) {
  switch (code) {
    case 'delete': onDelete(row); break;
    case 'edit': onEdit(row); break;
    case 'bom': onBomManage(row); break;
  }
}

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions: {
    schema: useSearchSchema(),
  },
  gridOptions: {
    columns: useColumns(onActionClick),
    height: 'auto',
    keepSource: true,
    pagerConfig: { enabled: true },
    proxyConfig: {
      ajax: {
        query: async ({ page }, formValues) => {
          return await getMaterialPage({
            page: page.currentPage,
            pageSize: page.pageSize,
            ...formValues,
          });
        },
      },
    },
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: true,
    },
  } as VxeTableGridOptions,
});

function refreshGrid() {
  gridApi.query();
}
</script>

<template>
  <Page auto-content-height>
    <FormModal @success="refreshGrid" />
    <Grid :table-title="$t('mes.basadata.material.list')">
      <template #toolbar-tools>
        <Button type="primary" @click="onCreate">
          <Plus class="size-5" />
          {{ $t('ui.actionTitle.create', [$t('mes.basadata.material.name')]) }}
        </Button>
      </template>
    </Grid>
    <BomPanel
      :product-id="selectedProduct?.id"
      :product-name="selectedProduct?.name"
      @refresh="refreshGrid"
    />
  </Page>
</template>
