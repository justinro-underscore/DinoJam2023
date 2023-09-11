using System.Collections;
using System.Collections.Generic;

public class LevelData
{
    public int starRating;

    public LevelData(int starRating)
    {
        if (starRating > Constants.MAX_STAR_RATING)
        {
            starRating = Constants.MAX_STAR_RATING;
        }
        else if (starRating < 0)
        {
            starRating = 0;
        }

        this.starRating = starRating;
    }
}

