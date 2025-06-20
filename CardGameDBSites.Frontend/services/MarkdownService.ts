export async function GetMarkdown(text: string) {
    const result = await $fetch<string>(
      "https://localhost:44344/umbraco/api/markdown/preview",
      {
        method: "POST",
        body: JSON.stringify({
            markdown: text
        })
      }
    );
    return result;
}