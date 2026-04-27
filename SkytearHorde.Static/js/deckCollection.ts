interface DeckProgressResult {
  deckId: number;
  progress: number;
}

function loadDeckCollectionProgress() {
  const elements = document.querySelectorAll<HTMLElement>('[data-deck-collection]');
  if (elements.length === 0) return;

  const deckIds = Array.from(elements)
    .map((el) => el.getAttribute('data-deck-collection'))
    .filter((id) => id !== null);

  const queryString = deckIds.map((id) => `deckIds=${id}`).join('&');
  fetch(`/umbraco/api/collection/getdecksprogress?${queryString}`)
    .then((res) => res.json())
    .then((data: DeckProgressResult[]) => {
      data.forEach((item) => {
        const el = document.querySelector(`[data-deck-collection="${item.deckId}"]`);
        if (el) {
          el.textContent = `Collection: ${item.progress.toFixed(2)}%`;
        }
      });
    })
    .catch((err) => {
      console.error('Failed to load deck collection progress:', err);
    });
}

export default () => {
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', loadDeckCollectionProgress);
  } else {
    loadDeckCollectionProgress();
  }
  window.addEventListener('deck-overview-updated', loadDeckCollectionProgress);
};
