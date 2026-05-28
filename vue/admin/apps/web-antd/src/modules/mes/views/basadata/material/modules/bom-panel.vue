<script lang="ts" setup>
import { h, ref, watch, computed } from 'vue';
import { Card, Tabs, message, Modal as AntModal, Tag, Button, Space, Table } from 'ant-design-vue';

import { $t } from '#/locales';
import { useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import type { BomApi } from '#/modules/mes/api/bom';
import {
  getBomPage,
  deleteBom,
  copyBom,
  releaseBom,
  rollbackBom,
  getBomDetail,
  getVersionHistory,
  getBomDiff,
  deleteBomItem,
} from '#/modules/mes/api/bom';

import BomForm from './bom-form.vue';
import BomItemForm from './bom-item-form.vue';

const props = defineProps<{
  productId?: number;
  productName?: string;
}>();

const emit = defineEmits(['refresh']);

const activeTab = ref('bomList');
const bomList = ref<BomApi.BomItem[]>([]);
const loading = ref(false);
const selectedBom = ref<BomApi.BomItem>();
const bomDetail = ref<BomApi.BomDetail>();
const versionHistory = ref<BomApi.VersionHistoryItem[]>([]);
const bomPage = ref({ page: 1, pageSize: 50, total: 0 });
const bomItems = ref<BomApi.BomItemDetail[]>([]);
const diffResult = ref<BomApi.BomDiffResult>();
const selectedHistoryIds = ref<number[]>([]);
const previewDetail = ref<BomApi.BomDetail>();

// BOM Form Modal
const [BomFormModal, bomFormModalApi] = useVbenModal({
  connectedComponent: BomForm,
  destroyOnClose: true,
});

// BOM Item Form Modal
const [BomItemFormModal, bomItemFormModalApi] = useVbenModal({
  connectedComponent: BomItemForm,
  destroyOnClose: true,
});

const bomStatusOptions: Record<number, { color: string; text: string }> = {
  0: { color: 'default', text: $t('bom.statusDraft') },
  1: { color: 'success', text: $t('bom.statusReleased') },
  2: { color: 'error', text: $t('bom.statusObsolete') },
  3: { color: 'warning', text: $t('bom.statusRevisionPending') },
};

async function loadBomList() {
  if (!props.productId) return;
  loading.value = true;
  try {
    const res = await getBomPage({
      page: bomPage.value.page,
      pageSize: bomPage.value.pageSize,
      productId: props.productId,
    });
    bomList.value = (res as any)?.items || [];
    bomPage.value.total = (res as any)?.total || 0;
  } finally {
    loading.value = false;
  }
}

async function loadBomDetail(bomId: number) {
  try {
    const detail = await getBomDetail(bomId);
    bomDetail.value = detail;
    bomItems.value = detail?.items || [];
  } catch {
    bomDetail.value = undefined;
    bomItems.value = [];
  }
}

async function loadVersionHistory(bomId: number) {
  try {
    versionHistory.value = await getVersionHistory(bomId) || [];
  } catch {
    versionHistory.value = [];
  }
}

watch(() => props.productId, (val) => {
  if (val) {
    bomList.value = [];
    selectedBom.value = undefined;
    bomDetail.value = undefined;
    bomItems.value = [];
    versionHistory.value = [];
    diffResult.value = undefined;
    activeTab.value = 'bomList';
    loadBomList();
  }
});

function onNewBom() {
  bomFormModalApi.setData(null).open();
}

function onEditBom(row: BomApi.BomItem) {
  bomFormModalApi.setData(row).open();
}

function onCopyBom(row: BomApi.BomItem) {
  const hide = message.loading({ content: $t('common.loadingText'), duration: 0 });
  copyBom(row.id!)
    .then(() => {
      message.success($t('ui.actionMessage.copySuccess'));
      loadBomList();
    })
    .finally(() => hide());
}

function onDeleteBom(row: BomApi.BomItem) {
  AntModal.confirm({
    title: $t('ui.actionMessage.deleteConfirm', [row.name || row.code]),
    onOk: () => deleteBom(row.id!).then(() => {
      message.success($t('ui.actionMessage.deleteSuccess'));
      loadBomList();
      if (selectedBom.value?.id === row.id) {
        selectedBom.value = undefined;
        bomDetail.value = undefined;
        bomItems.value = [];
      }
    }),
  });
}

function onReleaseBom(row: BomApi.BomItem) {
  AntModal.confirm({
    title: $t('bom.releaseBom'),
    content: $t('ui.actionMessage.confirmAction'),
    onOk: async () => {
      await releaseBom(row.id!);
      message.success($t('ui.actionMessage.success'));
      loadBomList();
      if (selectedBom.value?.id === row.id) loadBomDetail(row.id!);
    },
  });
}

function onSelectBom(row: BomApi.BomItem) {
  selectedBom.value = row;
  activeTab.value = 'bomDetail';
  loadBomDetail(row.id!);
  loadVersionHistory(row.id!);
}

function onBomFormSuccess() {
  loadBomList();
  bomFormModalApi.close();
}

function onNewBomItem() {
  bomItemFormModalApi.setData(null).open();
}

function onEditBomItem(row: BomApi.BomItemDetail) {
  bomItemFormModalApi.setData(row).open();
}

function onDeleteBomItem(row: BomApi.BomItemDetail) {
  AntModal.confirm({
    title: $t('ui.actionMessage.deleteConfirm', [row.itemName || '']),
    onOk: () => deleteBomItem(row.id!).then(() => {
      message.success($t('ui.actionMessage.deleteSuccess'));
      loadBomDetail(selectedBom.value?.id!);
    }),
  });
}

function onBomItemFormSuccess() {
  loadBomDetail(selectedBom.value?.id!);
  bomItemFormModalApi.close();
}

function onViewHistory(bomId: number) {
  selectedBom.value = bomList.value.find(b => b.id === bomId);
  loadVersionHistory(bomId);
  loadBomDetail(bomId);
  activeTab.value = 'versionHistory';
}

function onRollback(bomId: number, historyId: number) {
  AntModal.confirm({
    title: $t('bom.rollbackBom'),
    content: $t('ui.actionMessage.confirmAction'),
    onOk: async () => {
      await rollbackBom(bomId, historyId);
      message.success($t('ui.actionMessage.success'));
      loadBomList();
      activeTab.value = 'bomList';
    },
  });
}

function onCompareVersions(h1: number, h2: number) {
  getBomDiff(h1, h2).then(res => {
    diffResult.value = res;
    activeTab.value = 'diffResult';
  });
}

function onPreviewVersion(historyId: number) {
  getBomDetail(historyId).then(detail => {
    previewDetail.value = detail;
    message.info($t('bom.preview') + ' - ' + (detail?.header?.version || ''));
  });
}

const bomColumns = [
  { dataIndex: 'code', title: $t('bom.code'), width: 130 },
  { dataIndex: 'version', title: $t('bom.version'), width: 80 },
  {
    dataIndex: 'status',
    title: $t('bom.status'),
    width: 100,
    customRender: ({ value }: any) => {
      const opt = bomStatusOptions[value];
      return opt ? h(Tag, { color: opt.color }, () => opt.text) : value;
    },
  },
  { dataIndex: 'effDate', title: $t('bom.effDate'), width: 110 },
  { dataIndex: 'expDate', title: $t('bom.expDate'), width: 110 },
  {
    dataIndex: 'operation',
    title: $t('mes.basadata.material.operation'),
    width: 320,
    fixed: 'right',
    customRender: ({ record }: any) => {
      const canEdit = record.status === 0;
      const isDraft = record.status === 0;
      return h(Space, {}, () => [
        h(Button, { size: 'small', type: 'link', onClick: () => onSelectBom(record) }, () => $t('bom.detail')),
        h(Button, { size: 'small', type: 'link', disabled: !canEdit, onClick: () => onEditBom(record) }, () => $t('common.edit')),
        h(Button, { size: 'small', type: 'link', onClick: () => onCopyBom(record) }, () => $t('bom.copyBom')),
        h(Button, { size: 'small', type: 'link', disabled: !isDraft, onClick: () => onReleaseBom(record) }, () => $t('bom.releaseBom')),
        h(Button, { size: 'small', type: 'link', danger: true, disabled: !canEdit, onClick: () => onDeleteBom(record) }, () => $t('common.delete')),
        h(Button, { size: 'small', type: 'link', onClick: () => onViewHistory(record.id) }, () => $t('bom.versionHistory')),
      ]);
    },
  },
];

const itemColumns = [
  { dataIndex: 'sort', title: $t('bom.sort'), width: 60 },
  { dataIndex: 'itemCode', title: $t('bom.itemCode'), width: 120 },
  { dataIndex: 'itemName', title: $t('bom.itemName'), width: 150 },
  { dataIndex: 'itemSpec', title: $t('bom.itemSpec'), width: 120 },
  { dataIndex: 'qty', title: $t('bom.qty'), width: 80 },
  { dataIndex: 'scrapRate', title: $t('bom.scrapRate'), width: 90 },
  { dataIndex: 'substituteIds', title: $t('bom.substituteIds'), width: 120 },
  { dataIndex: 'remark', title: $t('bom.remark'), width: 150 },
  {
    dataIndex: 'operation',
    title: $t('mes.basadata.material.operation'),
    width: 150,
    fixed: 'right',
    customRender: ({ record }: any) => {
      return h(Space, {}, () => [
        h(Button, { size: 'small', type: 'link', onClick: () => onEditBomItem(record) }, () => $t('common.edit')),
        h(Button, { size: 'small', type: 'link', danger: true, onClick: () => onDeleteBomItem(record) }, () => $t('common.delete')),
      ]);
    },
  },
];

const historyColumns = [
  { dataIndex: 'version', title: $t('bom.version'), width: 100 },
  { dataIndex: 'changeDescription', title: $t('bom.changeDescription'), width: 200 },
  { dataIndex: 'creationTime', title: $t('common.createdAt'), width: 170 },
  {
    dataIndex: 'operation',
    title: $t('mes.basadata.material.operation'),
    width: 200,
    customRender: ({ record }: any) => {
      return h(Space, {}, () => [
        h(Button, { size: 'small', type: 'link', onClick: () => onPreviewVersion(record.id) }, () => $t('bom.preview')),
        h(Button, { size: 'small', type: 'link', danger: true, onClick: () => onRollback(selectedBom.value?.id, record.id) }, () => $t('bom.rollbackBom')),
      ]);
    },
  },
];
</script>

<template>
  <div class="mt-4">
    <Card v-if="productId" :title="`${$t('bom.selectedProduct')}：${productName || ''}`" size="small">
      <template #extra>
        <Space>
          <Button type="primary" size="small" @click="onNewBom">
            <template #icon><Plus /></template>
            {{ $t('bom.createBom') }}
          </Button>
        </Space>
      </template>

      <Tabs v-model:activeKey="activeTab">
        <TabPane key="bomList" :tab="$t('bom.list')">
          <Table
            :columns="bomColumns"
            :data-source="bomList"
            :loading="loading"
            :pagination="false"
            row-key="id"
            size="small"
            bordered
          />
        </TabPane>

        <TabPane key="bomDetail" :tab="$t('bom.detail')" :disabled="!selectedBom">
          <template v-if="selectedBom">
            <div class="mb-2 flex items-center gap-4">
              <span><strong>{{ $t('bom.code') }}：</strong>{{ selectedBom.code }}</span>
              <span><strong>{{ $t('bom.version') }}：</strong>{{ selectedBom.version }}</span>
              <Tag :color="bomStatusOptions[selectedBom.status]?.color">
                {{ bomStatusOptions[selectedBom.status]?.text }}
              </Tag>
              <span v-if="selectedBom.effDate"><strong>{{ $t('bom.effDate') }}：</strong>{{ selectedBom.effDate }}</span>
              <Button size="small" type="primary" @click="onNewBomItem">
                <template #icon><Plus /></template>
                {{ $t('bom.addItem') }}
              </Button>
            </div>
            <Table
              :columns="itemColumns"
              :data-source="bomItems"
              :pagination="false"
              row-key="id"
              size="small"
              bordered
            />
          </template>
        </TabPane>

        <TabPane key="versionHistory" :tab="$t('bom.versionHistory')" :disabled="!selectedBom">
          <template v-if="versionHistory.length">
            <Table
              :columns="historyColumns"
              :data-source="versionHistory"
              :pagination="false"
              row-key="id"
              size="small"
              bordered
            />
          </template>
          <div v-else class="py-8 text-center text-gray-400">
            {{ $t('common.noData') }}
          </div>
        </TabPane>

        <TabPane key="diffResult" :tab="$t('bom.diffBom')" :disabled="!diffResult">
          <template v-if="diffResult">
            <div class="mb-2">
              <strong>{{ $t('bom.headerDiff') }}：</strong>
              <span>{{ diffResult.headerDiff }}</span>
            </div>
            <div v-if="diffResult.itemChanges.length">
              <strong>{{ $t('bom.itemChanges') }}：</strong>
              <div v-for="(item, idx) in diffResult.itemChanges" :key="idx" class="ml-4 text-sm">
                <Tag :color="item.changeType === '新增' ? 'green' : item.changeType === '删除' ? 'red' : 'orange'">
                  {{ item.changeType }}
                </Tag>
                <span>Item#{{ item.itemId }}</span>
                <span v-if="item.oldQty != null"> 用量: {{ item.oldQty }} → {{ item.newQty }}</span>
                <span v-if="item.oldScrapRate != null"> 损耗率: {{ item.oldScrapRate }}% → {{ item.newScrapRate }}%</span>
              </div>
            </div>
          </template>
        </TabPane>
      </Tabs>
    </Card>

    <div v-else class="mt-4 rounded-lg border border-dashed border-gray-300 py-16 text-center text-gray-400">
      {{ $t('common.selectFirst') }}
    </div>

    <BomFormModal :product-id="productId" @success="onBomFormSuccess" />
    <BomItemFormModal :bom-id="selectedBom?.id" @success="onBomItemFormSuccess" />
  </div>
</template>
