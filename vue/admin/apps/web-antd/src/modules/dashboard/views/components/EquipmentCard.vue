<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { Card, Statistic } from 'ant-design-vue';
import { getEquipmentStatusOverview } from '#/modules/dashboard/api';

import { $t } from '#/locales';

const running = ref<number>(0);
const stopped = ref<number>(0);
const maintenance = ref<number>(0);
const scrapped = ref<number>(0);
const loading = ref(false);

async function loadData() {
  loading.value = true;
  try {
    const data = await getEquipmentStatusOverview();
    running.value = data?.running || 0;
    stopped.value = data?.stopped || 0;
    maintenance.value = data?.maintenance || 0;
    scrapped.value = data?.scrapped || 0;
  } catch (error) {
    console.error('Failed to load equipment status:', error);
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  loadData();
});
</script>

<template>
  <Card class="shadow-sm" :loading="loading">
    <template #title>
      <div class="flex items-center">
        <span class="text-2xl mr-2">🔧</span>
        {{ $t('dashboard.equipmentStatus') }}
      </div>
    </template>
    <div class="grid grid-cols-2 gap-2">
      <div class="text-center p-2 bg-green-50 rounded">
        <div class="text-xl font-bold text-green-600">{{ running }}</div>
        <div class="text-sm text-gray-600">{{ $t('dashboard.running') }}</div>
      </div>
      <div class="text-center p-2 bg-red-50 rounded">
        <div class="text-xl font-bold text-red-600">{{ stopped }}</div>
        <div class="text-sm text-gray-600">{{ $t('dashboard.stopped') }}</div>
      </div>
      <div class="text-center p-2 bg-yellow-50 rounded">
        <div class="text-xl font-bold text-yellow-600">{{ maintenance }}</div>
        <div class="text-sm text-gray-600">{{ $t('dashboard.maintenance') }}</div>
      </div>
      <div class="text-center p-2 bg-gray-50 rounded">
        <div class="text-xl font-bold text-gray-600">{{ scrapped }}</div>
        <div class="text-sm text-gray-600">{{ $t('dashboard.scrapped') }}</div>
      </div>
    </div>
  </Card>
</template>
