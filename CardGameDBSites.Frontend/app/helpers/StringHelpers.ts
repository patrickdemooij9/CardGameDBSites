export function firstCharUppercase(value: string) {
  return value.charAt(0).toUpperCase() + value.substring(1);
}

export function formatPrice(value: number) {
  return `$${value.toFixed(2)}`;
}
