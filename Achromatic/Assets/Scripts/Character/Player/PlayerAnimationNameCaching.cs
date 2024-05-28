using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerAnimationNameCaching
{
    public static readonly string[] SWORD_ONOFF_ANIMATION = { "mode/weapon/sword_on", "mode/weapon/sword_off" };
    public static readonly string IDLE_ANIMATION = "idle";
    public static readonly string RUN_ANIMATION = "move/run";
    public static readonly string PARRY_ANIMATION = "fallimg/falling_success";
    public static readonly string[] JUMP_ANIMATION = { "move/jump", "move/jump_idle" };
    public static readonly string RANDING_ANIMATION = "move/randing";
    public static readonly string[] HIT_ANIMATIONS = {"battle/hit/hit_1", "battle/hit/hit_2", "battle/hit/hit_3" };
    public static readonly string[,] ATTACK_ANIMATION =
        { 
        { "battle/attack/slash/slash_side_1", "battle/attack/slash/slash_side_2" }, 
        { "battle/attack/slash/slash_top", "" }, 
        { "battle/attack/slash/slash_bottom","" } 
    }; // side, up, down
    public static readonly string[] DASH_ANIMATION = { "dash/dash/dash_left", "dash/dash/dash_up", "dash/dash/dash_down" };
}
