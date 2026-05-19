<script lang="ts" setup>
import type {
  OnActionClickParams,
  VxeTableGridOptions,
} from '#/adapter/vxe-table';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { Page, useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';
import { Button, message } from 'ant-design-vue';

import { $t } from '#/locales';

import type { AgentManageApi } from '#/modules/ai/api/agentManage';
import { deleteAgentManage, getAgentManagePage } from '#/modules/ai/api/agentManage';

import { useColumns } from './data';
import Form from './modules/form.vue';


const [FormModal, formModalApi] = useVbenModal({
  connectedComponent: Form,
  destroyOnClose: true,
});
/**
 * 编辑
 * @param row
 */
function onEdit(row: AgentManageApi.AgentManage) {
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
function onDelete(row: AgentManageApi.AgentManage) {
  const hideLoading = message.loading({
    content: $t('ui.actionMessage.deleting', [row.name]),
    duration: 0,
    key: 'action_process_msg',
  });
  if (!row.id) {
    message.error({
      content: $t('ui.actionMessage.deleteFailed', [row.name]),
    });
    return;
  }
  deleteAgentManage(row.id)
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
}: OnActionClickParams<AgentManageApi.AgentManage>) {
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
          return await getAgentManagePage({
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
    <Grid :table-title="$t('ai.agentManage.list')">
      <template #toolbar-tools>
        <Button type="primary" @click="onCreate">
          <Plus class="size-5" />
            {{ $t('ui.actionTitle.create', [$t('ai.agentManage.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
