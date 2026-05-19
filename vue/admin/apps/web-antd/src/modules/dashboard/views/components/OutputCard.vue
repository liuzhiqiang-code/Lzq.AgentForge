<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { Card, Statistic } from 'ant-design-vue';
import { getProductionOutput } from '#/modules/dashboard/api';

import { $t } from '#/locales';

const props = defineProps<{
  lineId?: number;
}>();

const todayOutput = ref<number>(0);
const weekOutput = ref<number>(0);
const loading = ref(false);

async function loadData() {
  loading.value = true;
  try {
    const data = await getProductionOutput({ lineId: props.lineId });
    todayOutput.value = data?.todayOutput || 0;
    weekOutput.value = data?.weekOutput || 0;
  } catch (error) {
    console.error('Failed to load production output:', error);
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
        <span class="text-2xl mr-2">🏭</span>
        {{ $t('dashboard.productionOutput') }}
      </div>
    </template>
    <Statistic :value="todayOutput" :title="$t('dashboard.todayOutput')" />
    <Statistic :value="weekOutput" :title="$t('dashboard.weekOutput')" class="mt-2" />
  </Card>
</template>
