<script lang="ts" setup>
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { Page } from '@vben/common-ui';

import { getInspectionRecordPage } from '#/modules/equipment/api/inspection';

import { useColumns } from './data';

const [Grid] = useVbenVxeGrid({
  gridEvents: {},
  gridOptions: {
    columns: useColumns(),
    height: 'auto',
    keepSource: true,
    pagerConfig: { enabled: true },
    proxyConfig: {
      ajax: {
        query: async ({ page }, formValues) => {
          return await getInspectionRecordPage({
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
</script>
<template>
  <Page auto-content-height>
    <Grid :table-title="$t('equipment.inspectionRecord.list')" />
  </Page>
</template>
