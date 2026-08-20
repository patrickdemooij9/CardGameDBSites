export type FaqEntry = {
  question: string;
  answer: string;
};

export function buildFaqSchema(entries: FaqEntry[], pageUrl: string): Record<string, unknown> | null {
  if (entries.length === 0) return null;

  return {
    "@type": "FAQPage",
    "@id": `${pageUrl}#faq`,
    url: pageUrl,
    mainEntity: entries.map((entry) => ({
      "@type": "Question",
      name: entry.question,
      acceptedAnswer: {
        "@type": "Answer",
        text: entry.answer.replace(/<[^>]*>/g, " ").replace(/\s+/g, " ").trim(),
      },
    })),
  };
}
