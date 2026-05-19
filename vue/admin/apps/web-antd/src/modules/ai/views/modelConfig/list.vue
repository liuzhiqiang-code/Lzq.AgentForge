<script lang="ts" setup>
import type {
  OnActionClickParams,
  VxeTableGridOptions,
} from '#/adapter/vxe-table';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { Page, useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';
import {Switch, Button, message } from 'ant-design-vue';

import { $t } from '#/locales';

import type { ModelConfigApi } from '#/modules/ai/api/modelConfig';
import { deleteModelConfig, getModelConfigPage } from '#/modules/ai/api/modelConfig';

import { useColumns } from './data';
import { providerMap } from '#/modules/ai/views/apiKey/data';
import Form from './modules/form.vue';


const [FormModal, formModalApi] = useVbenModal({
  connectedComponent: Form,
  destroyOnClose: true,
});
/**
 * 编辑
 * @param row
 */
function onEdit(row: ModelConfigApi.ModelConfig) {
  formModalApi.setData(row).open();
}
/**
 * 创建新
 */
function onCreate() {
  formModalApi.setData(null).open();
}
/**
 * 删除
 * @param row
 */
function onDelete(row: ModelConfigApi.ModelConfig) {
  const hideLoading = message.loading({
    content: $t('ui.actionMessage.deleting', [row.name]),
    duration: 0,
    key: 'action_process_msg',
  });
  deleteModelConfig(row.id)
    .then(() => {
      message.success({
        content: $t('ui.actionMessage.deleteSuccess', [row.name]),
        key: 'action_process_msg',
      });
      refreshGrid();
    })
    .catch(() => {
      hideLoading();
    });
}
/**
 * 表格操作按钮的回调函数
 */
function onActionClick({
  code,
  row,
}: OnActionClickParams<ModelConfigApi.ModelConfig>) {
  switch (code) {
    case 'delete': {
      onDelete(row);
      break;
    }
    case 'edit': {
      onEdit(row);
      break;
    }
  }
}
const [Grid, gridApi] = useVbenVxeGrid({
  gridEvents: {},
  gridOptions: {
    columns: useColumns(onActionClick),
    height: 'auto',
    keepSource: true,
    pagerConfig: {
      enabled: true,
    },
    proxyConfig: {
      ajax: {
        query: async ({ page }, formValues) => {
          return await getModelConfigPage({
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
      zoom: true,
    },
  } as VxeTableGridOptions,
});

/**
 * 刷新表格
 */
function refreshGrid() {
  gridApi.query();
}

</script>
<template>
  <Page auto-content-height>
    <FormModal @success="refreshGrid" />
    <Grid :table-title="$t('ai.modelConfig.list')">
       <!-- 厂商名称插槽 -->
      <template #provider="{ row }">
        {{ providerMap[row.provider] || row.provider }}
      </template>

      <!-- 是否启用插槽（只读开关） -->
      <template #isEnabled="{ row }">
        <Switch :checked="row.isEnabled" disabled />
      </template>
      <template #toolbar-tools>
        <Button type="primary" @click="onCreate">
          <Plus class="size-5" />
          {{ $t('ui.actionTitle.create', [$t('ai.modelConfig.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
