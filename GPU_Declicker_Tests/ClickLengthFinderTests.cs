﻿using GPU_Declicker_UWP_0._01;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GPU_Declicker_Tests
{
    [TestClass]
    public class ClickLengthFinderTests
    {
        [TestMethod]
        public void FindLengthOfClick_DamagedConstantInput_ReturnsLength()
        {
            const int historyLength = 512;
            const int signalLength = 5 * historyLength;
            const int damageLength = 10;
            const int damageStartPosition = signalLength / 2 ;

            float[] inputAudio = new float[signalLength];

            for (int index = 0; index < inputAudio.Length; index++)
                inputAudio[index] = 0.5f;

            for (int index = damageStartPosition;
                index < damageStartPosition + damageLength;
                index++)
                inputAudio[index] = 0;

            AudioData audioData =
                new AudioDataMono(inputAudio);

            for (int index = historyLength; index < audioData.LengthSamples() - historyLength; index++)
            {
                audioData.SetOutputSample(
                    index,
                    audioData.GetInputSample(index));

                float prediction = ClickRepairer.CalcBurgPred(
                    audioData,
                    index);

                audioData.SetPredictionErr(index,
                    prediction - audioData.GetInputSample(index));
            }

            HelperCalculator.CalculateErrorAverageCPU(
                audioData,
                historyLength,
                audioData.LengthSamples(),
                historyLength);

            audioData.SetCurrentChannelIsPreprocessed();
            audioData.BackupCurrentChannelPredErrors();

            var result = ClickLengthFinder.FindLengthOfClick(audioData, damageStartPosition, 250, 0);

            Assert.AreEqual(damageLength, result.Length);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(3)]
        [DataRow(6)]
        [DataRow(9)]
        [DataRow(12)]
        [DataRow(15)]
        [DataRow(18)]
        [DataRow(21)]
        [DataRow(24)]
        [DataRow(27)]
        [DataRow(30)]
        public void FindLengthOfClick_DamagedSinusoidalInput_ReturnsLength(int shift)
        {
            const int historyLength = 512;
            const int signalLength = 5 * historyLength;
            const int damageLength = 10;
            int damageStartPosition = signalLength / 2 + shift;

            float[] inputAudio = new float[signalLength];

            for (int index = 0; index < inputAudio.Length; index++)
                inputAudio[index] = 
                    (float)Math.Sin(2 * Math.PI * index / (historyLength / 5.2));

            for (int index = damageStartPosition;
                index < damageStartPosition + damageLength;
                index++)
                inputAudio[index] = 0;

            AudioData audioData =
                new AudioDataMono(inputAudio);

            for (int index = historyLength; 
                index < audioData.LengthSamples() - historyLength; 
                index++)
            {
                audioData.SetOutputSample(
                    index,
                    audioData.GetInputSample(index));

                float prediction = ClickRepairer.CalcBurgPred(
                    audioData,
                    index);

                audioData.SetPredictionErr(index,
                    prediction - audioData.GetInputSample(index));
            }

            HelperCalculator.CalculateErrorAverageCPU(
                audioData,
                historyLength,
                audioData.LengthSamples(),
                historyLength);

            audioData.SetCurrentChannelIsPreprocessed();
            audioData.BackupCurrentChannelPredErrors();

            var result = ClickLengthFinder.FindLengthOfClick(audioData, damageStartPosition, 250, 0);

            Assert.AreEqual(damageLength, result.Length);
        }

    }
}