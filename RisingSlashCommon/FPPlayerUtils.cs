using System;
using System.Reflection;

namespace RisingSlash.FP2Mods.RisingSlashCommon;

public static class FPPlayerUtils
{
    public static FPObjectState delFPEnemyHealthDrain;
    
    //public delegate void FPObjectState();
    /*
    public static FPObjectState FunctionToFPObjectState(Func<void> fun)
    {
        MethodInfo st_run_attack = merga.GetType().GetMethod("State_RunAttack", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        //dynMethod.Invoke(this, new object[] { methodParams });
        FPObjectState d_st_run_attack;
        d_st_run_attack = (FPObjectState) st_run_attack.CreateDelegate(typeof(FPObjectState), merga);
        return objectState;
    }
    */

    public static void GetPrivateMethodAsDelegate()
    {
        
    }

    public static T GetPrivateField<T, T2>(string fieldName, T2 instance)
    {
        FieldInfo info = instance.GetType().GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (T) info.GetValue(instance);
    }
    
    public static T GetPrivateStaticField<T>(string fieldName, Type type)
    {
        FieldInfo info = type.GetType().GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Static);
        return (T) info.GetValue(type);
    }

    public static void SetPrivateField<T, T2>(string fieldName, T2 instance, T value)
    {
        FieldInfo info = instance.GetType().GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Instance);
        info.SetValue(instance, value);
    }
    
    public static void SetPrivateStaticField<T>(string fieldName, Type type, T value)
    {
        FieldInfo info = type.GetType().GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Static);
        info.SetValue(type, value);
    }

    public static void HealthDrain(this FPBaseEnemy enemy, FPCharacterID characterID )
    {
        if (delFPEnemyHealthDrain == null) return;

        MethodInfo eneHealthDrain = enemy.GetType().GetMethod("HealthDrain", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        //dynMethod.Invoke(this, new object[] { methodParams });
        
        // TODO: The below line was temporarily dummied out to allow the build to buiild. I don't remember what it was for.
        // delFPEnemyHealthDrain = (FPObjectState) eneHealthDrain.CreateDelegate(typeof(FPObjectState), enemy);
    }

}