const monthNames = [
  "January",
  "February",
  "March",
  "April",
  "May",
  "June",
  "July",
  "August",
  "September",
  "October",
  "November",
  "December",
];

export function ToHumanReadableText(date: Date) {
  const now = new Date();
  const dateDiffMs = now.getTime() - date.getTime();
  const days = Math.floor(dateDiffMs / (1000 * 60 * 60 * 24));

  if (days === 0) return "Today";
  if (days === 1) return "Yesterday";
  if (days <= 30) return `${days} days ago`;

  // Format as "M" (month/day) and add year if different
  let result = `${monthNames[date.getMonth()]} ${date.getDate()}`;
  if (date.getFullYear() !== now.getFullYear()) {
    result = `${result} ${date.getFullYear()}`;
  }
  return result;
}

export function ParseToHumanReadableText(text: string) {
  return ToHumanReadableText(new Date(text));
}
