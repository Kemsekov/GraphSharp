public static class ManualTestData
{
    public static int[][] NodesConnections = new[]{
        new[]{0,4},new[]{0,1},
        new[]{1,2},new[]{1,3},
        new[]{1,8},new[]{2,7},
        new[]{2,9},new[]{3,7},
        new[]{4,5},new[]{5,0},
        new[]{5,1},new[]{5,3},
        new[]{5,6},new[]{6,3},
        new[]{7,1},new[]{7,9},
        new[]{8,9},new[]{9,3}
    };
    public static int[][][] ExpectedOrder =
    new[]{
        new[]{
            new[]{0, 7},
            new[]{1, 4, 9},
            new[]{2, 3, 5, 8},
            new[]{0, 1, 3, 6, 7, 9},
            new[]{1, 2, 3, 4, 7, 8, 9},
            new[]{1, 2, 3, 5, 7, 8, 9}
        },
        new[]{
            new[]{1,5},
            new[]{0,1,2,3,6,8},
            new[]{1,2,3,4,7,8,9},
            new[]{1,2,3,5,7,8,9},
            new[]{0,1,2,3,6,7,8,9},
            new[]{1,2,3,4,7,8,9}
        }
    };
}