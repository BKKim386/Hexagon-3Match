using UnityEngine;

public static class Util
{
    public static Vector3 AxialToWorldPos(Vector2Int axialPos, int radius)
    {
        return new Vector3(radius * (3f / 2f) * axialPos.x, radius * Mathf.Sqrt(3f) * (axialPos.y + axialPos.x / 2f));
    }

    public static Vector2Int WorldToAxialPos(Vector3 worldPos, int radius)
    {
        float x = worldPos.x / radius;
        float y = worldPos.y / radius;

        float q = (2f / 3f) * x;
        float r = (-1f / 3f) * x + (Mathf.Sqrt(3f) / 3f) * y;
        float s = -q - r;

        int rq = Mathf.RoundToInt(q);
        int rr = Mathf.RoundToInt(r);
        int rs = Mathf.RoundToInt(s);

        float qDiff = Mathf.Abs(rq - q);
        float rDiff = Mathf.Abs(rr - r);
        float sDiff = Mathf.Abs(rs - s);

        if (qDiff > rDiff && qDiff > sDiff)
            rq = -rr - rs;
        else if (rDiff > sDiff)
            rr = -rq - rs;
        else
            rs = -rq - rr;

        return new Vector2Int(rq, rr);
    }

    public static int GetAxialDistance(this Vector2Int v1,Vector2Int v2)
    {
        int dq = v1.x - v2.x;
        int dr = v1.y - v2.y;
        int ds = -dq - dr;

        return (Mathf.Abs(dq) + Mathf.Abs(dr) + Mathf.Abs(ds)) / 2;
    }

    public static class HexagonBlockColor
    {
        public static Color Red => new Color(1, 0, 0);
        public static Color Orange => new Color(1, 0.5f, 0);
        public static Color Yellow => new Color(1, 0.8f, 0);
        public static Color Green => new Color(0, 1, 0);
        public static Color Blue => new Color(0, 0, 1);
        public static Color Purple => new Color(0.5f, 0, 1);

        public static Color GetColor(Game.BlockColor blockColor)
        {
            return blockColor switch
            {
                Game.BlockColor.Red => Red,
                Game.BlockColor.Orange => Orange,
                Game.BlockColor.Yellow => Yellow,
                Game.BlockColor.Green => Green,
                Game.BlockColor.Blue => Blue,
                Game.BlockColor.Purple => Purple,
                _ => Color.white
            };
        }
    }
}
