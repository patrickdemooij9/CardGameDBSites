export type CardPriceChangeApiModel = {
    cardId: number;
    variantId: number | null;
    cardName: string;
    urlSegment: string;
    currentPrice: number;
    previousPrice: number;
    priceChange: number;
    priceChangePercent: number;
};
