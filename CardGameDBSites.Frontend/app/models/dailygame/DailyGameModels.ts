export interface DailyGameAttributeFeedback {
  name: string;
  matchType: string;
  guessValues: string[];
}

export interface DailyGameAttempt {
  attemptNumber: number;
  guessedCardId: number;
  guessedCardName?: string;
  isCorrect: boolean;
  feedback: DailyGameAttributeFeedback[];
  createdUtc: string;
}

export interface DailyGameLeaderboardEntry {
  rank: number;
  memberId?: number;
  memberName?: string;
  attemptsUsed: number;
  elapsedSeconds: number;
  solved: boolean;
  isCurrentPlayer: boolean;
}

export interface DailyGameBootstrap {
  guestSessionToken: string;
  maxAttempts: number;
  attemptsUsed: number;
  attemptsLeft: number;
  elapsedSeconds: number;
  blurLevel: number;
  isFinished: boolean;
  isSolved: boolean;
  attempts: DailyGameAttempt[];
  leaderboard: DailyGameLeaderboardEntry[];
  currentPlacement?: DailyGameLeaderboardEntry;
}

export interface DailyGameGuessPost {
  guessedCardId: number;
  guestSessionToken?: string;
}

export interface DailyGameGuessResult {
  state: DailyGameBootstrap;
  latestAttempt?: DailyGameAttempt;
}
