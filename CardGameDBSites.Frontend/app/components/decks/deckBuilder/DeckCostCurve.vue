<script setup lang="ts">
import { Bar } from "vue-chartjs";
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
} from "chart.js";
import type { CreateDeckModel } from "./models/CreateDeckModel";
import { computeCostCurveChart } from "~/helpers/CostCurveHelper";

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale);

const TYPE_COLORS = [
  "#4B6BFB",
  "#F5A623",
  "#E74C3C",
  "#2ECC71",
  "#9B59B6",
  "#1ABC9C",
  "#E67E22",
  "#3498DB",
  "#F39C12",
  "#D35400",
];

const props = defineProps<{
  deck: CreateDeckModel;
  costAttribute?: string;
  typeAttribute?: string;
}>();

const chartData = computed(() => {
  const raw = computeCostCurveChart(
    props.deck,
    props.costAttribute ?? "Cost",
    props.typeAttribute,
  );
  return {
    labels: raw.labels,
    datasets: raw.datasets.map((ds, i) => ({
      label: ds.label,
      data: ds.data,
      backgroundColor: TYPE_COLORS[i % TYPE_COLORS.length],
    })),
  };
});

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    title: {
      display: true,
      text: "Cost curve",
      color: "#111827",
      font: { size: 14, weight: "bold" as const },
    },
    legend: {
      display: true,
      labels: { color: "#374151" },
    },
  },
  scales: {
    x: {
      stacked: true,
      grid: { color: "rgba(0,0,0,0.08)" },
      ticks: { color: "#374151" },
    },
    y: {
      stacked: true,
      beginAtZero: true,
      grid: { color: "rgba(0,0,0,0.08)" },
      ticks: { color: "#374151", stepSize: 1 },
    },
  },
};
</script>

<template>
  <div class="mt-4 rounded border border-gray-200 bg-white p-2" style="height: 240px;">
    <ClientOnly>
      <Bar :data="chartData" :options="chartOptions" />
    </ClientOnly>
  </div>
</template>
