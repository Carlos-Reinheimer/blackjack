namespace Utils {
    public static class RunStats {
        
        public static int CurrentLevel;
        public static int CurrentScore;
        public static int CurrentLifeChips;

        public static void ClearRunStats() {
            CurrentLevel = 0;
            CurrentScore = 0;
            CurrentLifeChips = 0;
        }
    }
}
