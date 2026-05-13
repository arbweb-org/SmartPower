#ifndef MULTIMETER_H
#define MULTIMETER_H

#include <Arduino.h>
#include <EEPROM.h>

const int CURRENT_PINS[] = {A1, A3};
const int VOLTAGE_PIN = A5;
const int NUM_SAMPLES = 100;
const int SAMPLE_DELAY_US = 200;


struct PowerSamples {
    float voltageRMS;
    float current0RMS;
    float current1RMS;
};

class Multimeter
{
private:
    const int PARAM_ADDR = 100;

    float currentCal = 1.0;
    float voltageCal = 1.0;

public:
    struct Parameters
    {
        int StartFlag;
        float CurrentCal;
        float VoltageCal;
        int EndFlag;
    };

    String readRMS()
    {
        long sumV = 0, sumI0 = 0, sumI1 = 0;
        long sumSqV = 0, sumSqI0 = 0, sumSqI1 = 0;

        unsigned long startMicros = micros();

        for (int i = 0; i < NUM_SAMPLES; i++)
        {
            while (micros() - startMicros < (unsigned long)i * SAMPLE_DELAY_US)
                ;

            int vRaw = analogRead(VOLTAGE_PIN);
            int i0Raw = analogRead(CURRENT_PINS[0]);
            int i1Raw = analogRead(CURRENT_PINS[1]);

            sumV += vRaw;
            sumSqV += (long)vRaw * vRaw;
            sumI0 += i0Raw;
            sumSqI0 += (long)i0Raw * i0Raw;
            sumI1 += i1Raw;
            sumSqI1 += (long)i1Raw * i1Raw;
        }

        auto calculateRMS = [](long sum, long sumSq)
        {
            float mean = (float)sum / NUM_SAMPLES;
            float variance = ((float)sumSq / NUM_SAMPLES) - (mean * mean);
            return (variance > 0) ? sqrt(variance) : 0.0f;
        };

        // Calculate final calibrated values
        float vFinal = calculateRMS(sumV, sumSqV) * voltageCal;
        float i0Final = calculateRMS(sumI0, sumSqI0) * currentCal;
        float i1Final = calculateRMS(sumI1, sumSqI1) * currentCal;

        // Build the string: "Voltage|Current0|Current1"
        // The second parameter in String(value, decimalPlaces) controls precision
        String result = String(vFinal, 0) + "|" + String(i0Final, 2) + "|" + String(i1Final, 2);

        return result;
    }

    Parameters Params;

    Multimeter()
    {
        loadParameters();
    }

    void loop()
    {

    }

    Parameters getParameters()
    {
        Parameters params;
        EEPROM.get(PARAM_ADDR, params);

        return params;
    }

    bool saveParameters(Parameters params)
    {
        // 1. Generate a random "Transaction ID"
        // We use random(1, 32767) to avoid 0/0xFFFF which are common defaults
        int transactionID = random(1, 32767);

        params.StartFlag = transactionID;
        params.EndFlag = transactionID;

        // 2. Write to EEPROM
        EEPROM.put(PARAM_ADDR, params);

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

            currentCal = Params.CurrentCal; // Sync live variables
            voltageCal = Params.VoltageCal; // Sync live variables
            return;
        }

        Params.CurrentCal = 1.0;
        Params.VoltageCal = 1.0;

        saveParameters(Params); 
    }

    String updateCurrentCal(float newCal) {
        Params.CurrentCal = newCal;
        if (saveParameters(Params)) {
            currentCal = newCal; // Sync live variable used in calculations
            return "OK";
        }
        return "ERROR";
    }

    String updateVoltageCal(float newCal) {
        Params.VoltageCal = newCal;
        if (saveParameters(Params)) {
            voltageCal = newCal; // Sync live variable used in calculations
            return "OK";
        }
        return "ERROR";
    }
};

#endif