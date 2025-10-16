namespace Utils {
    public static class RunStats {
        
        public static int CurrentRound;
        public static int CurrentScore;
        public static int CurrentLifeChips;

        public static void ClearRunStats() {
            CurrentRound = 0;
            CurrentScore = 0;
            CurrentLifeChips = 0;
        }
    }
}
