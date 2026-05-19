<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { Card, Statistic, Progress } from 'ant-design-vue';
import { getCompletionRate } from '#/modules/dashboard/api';

import { $t } from '#/locales';

const props = defineProps<{
  lineId?: number;
}>();

const completionRate = ref<number>(0);
const dispatched = ref<number>(0);
const completed = ref<number>(0);
const loading = ref(false);

async function loadData() {
  loading.value = true;
  try {
    const data = await getCompletionRate({ lineId: props.lineId });
    completionRate.value = data?.rate || 0;
    dispatched.value = data?.dispatched || 0;
    completed.value = data?.completed || 0;
  } catch (error) {
    console.error('Failed to load completion rate:', error);
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
        <span class="text-2xl mr-2">📋</span>
        {{ $t('dashboard.completionRate') }}
      </div>
    </template>
    <Statistic
      :value="completionRate"
      :title="$t('dashboard.completionRateValue')"
      suffix="%"
    />
    <div class="mt-2 space-y-1">
      <div class="flex justify-between text-sm">
        <span>{{ $t('dashboard.dispatched') }}</span>
        <span>{{ dispatched }}</span>
      </div>
      <div class="flex justify-between text-sm">
        <span>{{ $t('dashboard.completed') }}</span>
        <span>{{ completed }}</span>
      </div>
    </div>
    <Progress :percent="completionRate" class="mt-2" />
  </Card>
</template>
