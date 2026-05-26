<script lang="ts" setup>
import type { EchartsUIType } from '@vben/plugins/echarts';
import { onMounted, ref } from 'vue';
import { EchartsUI, useEcharts } from '@vben/plugins/echarts';
import { getConversationTrends } from '#/api/dashboard/analytics';

const chartRef = ref<EchartsUIType>();
const { renderEcharts } = useEcharts(chartRef);

onMounted(async () => {
  // 从后端获取近7天每小时的对话量
  const { dates, conversations, apiCalls } = await getConversationTrends();

  renderEcharts({
    grid: { bottom: 0, containLabel: true, left: '1%', right: '1%', top: '2%' },
    tooltip: { trigger: 'axis' },
    xAxis: { type: 'category', data: dates },
    yAxis: { type: 'value' },
    series: [
      {
        name: '对话数',
        type: 'bar',
        data: conversations,
        itemStyle: { color: '#5ab1ef' },
      },
      {
        name: 'API 调用',
        type: 'line',
        data: apiCalls,
        itemStyle: { color: '#019680' },
        smooth: true,
      },
    ],
  });
});
</script>

<template>
  <EchartsUI ref="chartRef" />
</template>
