#ifndef REFRIGERATOR_H
#define REFRIGERATOR_H

#include <Arduino.h>
#include <EEPROM.h>

class Refrigerator
{
private:
    float _boxTemp = 0;
    float _evapTemp = 0;

    // State variables
    bool _defrostStopped = false;
    unsigned long startTime;
    unsigned long lastCompressorOff;

    // Helper to get elapsed seconds
    unsigned long timeElapsed()
    {
        return (millis() - startTime) / 1000;
    }

    void TurnCompressorOn()
    {
        // Check if DelayTime (in seconds) has passed since last off
        if ((millis() - lastCompressorOff) < (unsigned long)Params.DelayTime * 1000)
        {
            return;
        }
        CompressorOn = true;
    }

    void TurnCompressorOff()
    {
        if (!CompressorOn)
        { return; }

        CompressorOn = false;
        lastCompressorOff = millis();
    }

    void TurnDefrostOn() { DefrostOn = true; }
    void TurnDefrostOff() { DefrostOn = false; }

    void Cool()
    {
        float maxTemp = Params.TargetTemp + Params.Differential;
        if (_boxTemp >= maxTemp)
        {
            TurnCompressorOn();
        }
        else if (_boxTemp <= Params.TargetTemp)
        {
            TurnCompressorOff();
        }
    }

    void Defrost()
    {
        if (_defrostStopped)
        { return; }
            
        if (_evapTemp < Params.DefrostTemp)
        {
            TurnDefrostOn();
        }
        else
        {
            TurnDefrostOff();
            _defrostStopped = true;
        }
    }

public:
    struct Parameters
    {
        int StartFlag;
        int TargetTemp;
        int DefrostTemp;
        int Differential;
        int DelayTime;
        int CoolingDuration;
        int DefrostDuration;
        int EndFlag;
    };

    // Properties
    bool CompressorOn = false;
    bool DefrostOn = false;
    bool EvapFanOn = false;
    bool LightOn = false;

    Parameters Params;

    Refrigerator()
    {
        startTime = millis();
        lastCompressorOff = millis();
        loadParameters();
    }

    void loop(float boxTemp, float evapTemp)
    {
        _boxTemp = boxTemp;
        _evapTemp = evapTemp;

        unsigned long elapsed = timeElapsed();
        unsigned long coolingDuration = Params.CoolingDuration;
        unsigned long defrostDuration = Params.DefrostDuration;

        if (Params.DefrostDuration == 0) 
        {
            TurnDefrostOff();
            _defrostStopped = false;
        }
        else if (elapsed < coolingDuration)
        {
            Cool();
        }
        else if (elapsed < (coolingDuration + defrostDuration))
        {
            TurnCompressorOff();
            Defrost();
        }
        else
        {
            TurnDefrostOff();
            startTime = millis();
            _defrostStopped = false;
        }
    }

    Parameters getParameters()
    {
        Parameters params;
        EEPROM.get(0, params);

        return params;
    }

    bool saveParameters(Parameters params)
    {
        if (params.DelayTime < 3 * 60 || params.DelayTime > 10 * 60 || params.DefrostTemp < 4 || params.DefrostTemp > 15) 
        { return false; }

        // 1. Generate a random "Transaction ID"
        // We use random(1, 32767) to avoid 0/0xFFFF which are common defaults
        int transactionID = random(1, 32767);

        params.StartFlag = transactionID;
        params.EndFlag = transactionID;

        // 2. Write to EEPROM
        EEPROM.put(0, params);

        // 3. Verify integrity
        Parameters verified = getParameters();
        return (verified.StartFlag == transactionID && verified.EndFlag == transactionID);
    }

    // Runs only at startup to load parameters from EEPROM, or initialize defaults if invalid
    void loadParameters()
    {
        Parameters params = getParameters();

        // Valid if: bookends match AND it's not a fresh EEPROM (0xFFFF / -1)
        if (params.StartFlag == params.EndFlag && params.StartFlag > 0 && params.StartFlag != 0x7FFF) {
            Params = params;
            return;
        }

        Params.TargetTemp = 5;
        Params.DefrostTemp = 10;
        Params.Differential = 5;
        Params.DelayTime = 3 * 60;
        Params.CoolingDuration = 15 * 60;
        Params.DefrostDuration = 5 * 60;

        saveParameters(Params); 
    }
};

#endif