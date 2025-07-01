import { DoFetch } from "~/helpers/RequestsHelper";

export async function GetMarkdown(text: string) {
    const result = await DoFetch<string>(
      "/umbraco/api/markdown/preview",
      {
        method: "POST",
        body: JSON.stringify({
            markdown: text
        })
      }
    );
    return result;
}