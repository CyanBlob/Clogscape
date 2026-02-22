using System;

static class CombatLevel
{
    public static int GetCombatLvl(int atk, int str, int hps, int def, int rng, int mag, int pra)
    {
        int base_lvl = Convert.ToInt32(pra / 2 - 0.5 + hps + def) / 4;
        int melee = Convert.ToInt32((atk + str) * 0.325);
        int range = Convert.ToInt32((rng * 1.5 - 0.5) * 0.325);
        int mage = Convert.ToInt32((mag * 1.5 - 0.5) * 0.325);

        if (range > melee || mage > melee)
        {
            return range >= mage ? base_lvl + range : base_lvl + mage;
        }

        return base_lvl + melee;
    }
}