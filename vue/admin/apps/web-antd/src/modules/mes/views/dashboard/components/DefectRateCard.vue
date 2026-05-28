<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { Card, Statistic, Tag } from 'ant-design-vue';
import { getDefectRateStatistics } from '#/modules/mes/api';

import { $t } from '#/locales';

const props = defineProps<{
  lineId?: number;
}>();

const todayDefectRate = ref<number>(0);
const weekTrend = ref<string>('');
const loading = ref(false);

async function loadData() {
  loading.value = true;
  try {
    const data = await getDefectRateStatistics({ lineId: props.lineId });
    todayDefectRate.value = data?.todayDefectRate || 0;
    weekTrend.value = data?.weekTrend || 'stable';
  } catch (error) {
    console.error('Failed to load defect rate:', error);
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  loadData();
});

function getTrendColor(trend: string) {
  switch (trend) {
    case 'up':
      return 'red';
    case 'down':
      return 'green';
    default:
      return 'blue';
  }
}
</script>

<template>
  <Card class="shadow-sm" :loading="loading">
    <template #title>
      <div class="flex items-center">
        <span class="text-2xl mr-2">⚠️</span>
        {{ $t('mes.dashboard.defectRate') }}
      </div>
    </template>
    <Statistic
      :value="todayDefectRate"
      :title="$t('mes.dashboard.todayDefectRate')"
      suffix="%"
    />
    <div class="mt-2 flex items-center justify-between">
      <span class="text-sm text-gray-600">{{ $t('mes.dashboard.weekTrend') }}</span>
      <Tag :color="getTrendColor(weekTrend)">
        {{ weekTrend === 'up' ? '↑' : weekTrend === 'down' ? '↓' : '→' }}
        {{ $t('mes.dashboard.weekTrend') }}
      </Tag>
    </div>
  </Card>
</template>
