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
}
