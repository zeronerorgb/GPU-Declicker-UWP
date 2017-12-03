﻿using System;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GPU_Declicker_UWP_0._01
{
    public sealed partial class AudioViewer : UserControl
    {
        public AudioViewer()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// audio samples to view
        /// </summary>
        private AudioDataClass audioData = null;

        /// <summary>
        /// offset from beginning of audioData to beginning waveForm
        /// </summary>
        private int OffsetPosition = 0;

        /// <summary>
        /// magnification ratio
        /// when set to 1, waveForm is most detailed
        /// when set to R, waveForm drops each R-1 from R audioData samples
        /// </summary>
        private double audioDataToWaveFormRatio = 1;

        /// <summary>
        /// Last mouse pointer position touching waveForms
        /// Used to calculate new OffsetPosition when user slides waveForms
        /// </summary>
        public Point PointerLastPosition { get; internal set; }

        /// <summary>
        /// True when when user slides waveForms
        /// </summary>
        public bool IsMovingByMouse { get; internal set; }

        /// <summary>
        /// Fills this with AudioData, sets Ratio and OffsetPosition
        /// </summary>
        /// <param name="audioDataInput"></param>
        public void Fill(AudioDataClass audioDataInput)
        {
            OffsetPosition = 0;
            // Sets Ratio to show whole audio track
            audioDataToWaveFormRatio =
                audioDataInput.Length / waveFormLeftChannel.ActualWidth;
            audioData = audioDataInput;

            FillWaveForm();
        }

        /// <summary>
        /// move OffsetPositionX to the right for one waveForm length
        /// </summary>
        public void GoNextBigStep()
        {
            if (audioData == null) return;

            int deltaX = (int)waveFormLeftChannel.ActualWidth;
            GoNextX(deltaX);
        }

        // move OffsetPositionX to the right for one tenth of waveForm length
        public void GoNextSmalStep()
        {
            if (audioData == null) return;

            int deltaX = (int)waveFormLeftChannel.ActualWidth / 10;
            GoNextX(deltaX);
        }

        // move OffsetPositionX to the right for shiftX samples
        public void GoNextX(int deltaX)
        {
            if (audioData == null) return;
            // Calculate number of samples to shift
            int shift = (int)(deltaX * audioDataToWaveFormRatio);
            // Calculate number of samples that waveForms show
            int samplesOnScrean = (int)(
                waveFormLeftChannel.ActualWidth
                * audioDataToWaveFormRatio
                );
            // if there is enough room on the right than shift offsetPosition
            if (OffsetPosition + shift + samplesOnScrean < audioData.Length)
                OffsetPosition += shift;
            else
                // set OffsetPosition to show the end of audioData
                OffsetPosition = audioData.Length - samplesOnScrean - 1
                    - (int)audioDataToWaveFormRatio;

            FillWaveForm();
        }

        // move OffsetPositionX to the right for one waveForm length 
        public void GoPrevBigStep()
        {
            int deltaX = (int)waveFormLeftChannel.ActualWidth;
            GoPrevX(deltaX);
        }

        // move OffsetPositionX to the right for one tenth of waveForm length 
        public void GoPrevSmalStep()
        {
            int deltaX = (int)waveFormLeftChannel.ActualWidth / 10;
            GoPrevX(deltaX);
        }

        // move OffsetPositionX to the right for shiftX samples 
        public void GoPrevX(int X)
        {
            if (audioData == null) return;
            // Calculate number of samples to shift
            int shift = (int)(X * audioDataToWaveFormRatio);
            // if there is enough room on the left than shift offsetPositionX
            if (OffsetPosition > shift) OffsetPosition -= shift;
            else
                // set OffsetPositionX to show the begining of audioData
                OffsetPosition = 0;

            FillWaveForm();
        }

        /// <summary>
        /// sets points for PolyLine showing wave form
        /// </summary>
        private void FillWaveForm()
        {
            if (audioData == null) return;

            waveFormLeftChannel.Points.Clear();
            waveFormRightCnannel.Points.Clear();

            // for every x-axis position of waveForm
            for (int x = 0; x < waveFormsGroup.ActualWidth; x++)
            {
                audioData.CurrentChannel = Channel.Left;
                AddPoint(waveFormLeftChannel, x);

                if (audioData.IsStereo)
                {
                    audioData.CurrentChannel = Channel.Right;
                    AddPoint(waveFormRightCnannel, x);
                }
            }
        }

        /// <summary>
        /// Adds a point representing one or many samples to wave form
        /// </summary>
        private void AddPoint(Polyline waveForm, int x)
        {
            int offsetY = (int)waveFormLeftChannel.ActualHeight / 2;
            int start = OffsetPosition + (int)(x * audioDataToWaveFormRatio);
            if (start >= audioData.Length)
            {
                return;
            }
            int length = (int)audioDataToWaveFormRatio;

            // looks for max and min among many samples represented by a point on wave form
            FindMinMax(start, length, out float min, out float max);

            // connect previous point to a new point
            int y = (int)(-0.5 * waveFormLeftChannel.ActualHeight * max) + offsetY;
            waveForm.Points.Add(new Point(x, y));
            if (min != max)
            {
                // form vertical line connecting max and min
                y = (int)(-0.5 * waveForm.ActualHeight * min) + offsetY;
                waveForm.Points.Add(new Point(x, y));
            }
        }

        /// <summary>
        /// Looks for max and min among many samples represented by a point on wave form
        /// </summary>
        /// <param name="start">first sample position</param>
        /// <param name="length">number of samples</param>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        private void FindMinMax(int start, int length, out float min, out float max)
        {
            min = audioData.GetInputSample(start);
            max = audioData.GetInputSample(start);
            for (int j = 0; j < length && start + j < audioData.Length; j++)
            {
                if (audioData.GetInputSample(start + j) < min)
                    min = audioData.GetInputSample(start + j);
                if (audioData.GetInputSample(start + j) > max)
                    max = audioData.GetInputSample(start + j);
            }
        }

        /// <summary>
        /// Increases detalization on waveForm
        /// </summary>
        public void MagnifyMore()
        {
            if (audioDataToWaveFormRatio >= 2)
                audioDataToWaveFormRatio /= 2;
            else
                audioDataToWaveFormRatio = 1;

            FillWaveForm();
        }

        /// <summary>
        /// Decreases detalization on waveForm
        /// </summary>
        public void MagnifyLess()
        {
            if (waveFormLeftChannel.ActualWidth * audioDataToWaveFormRatio * 2
                < audioData.Length)
                audioDataToWaveFormRatio *= 2;
            else
                audioDataToWaveFormRatio =
                audioData.Length / waveFormLeftChannel.ActualWidth;

            FillWaveForm();
        }

        internal void AudioViewerSizeChanged()
        {
            OffsetPosition = 0;
            // Sets Ratio to show whole audio track
            if (audioData != null)
                audioDataToWaveFormRatio =
                    audioData.Length / waveFormsGroup.ActualWidth;

            FillWaveForm();
        }

        /// <summary>
        /// Returns offset in samples for pointer position
        /// </summary>
        /// <param name="pointerPosition"> pointer position on waveForm</param>
        public int PointerOffsetPosition(double pointerPosition)
            => (int)(pointerPosition * audioDataToWaveFormRatio)
                + OffsetPosition;

        /// <summary>
        /// Adjusts OffsetPosition to make pointer point to Offset sample
        /// </summary>
        /// <param name="Offset"> offset for pointer position</param>
        /// <param name="pointerPosition"> X of pointer position on waveForm</param>
        public void SetOffsetForPointer(int Offset, double pointerPosition)
        {
            OffsetPosition = Offset - (int)(pointerPosition * audioDataToWaveFormRatio);
            if (OffsetPosition < 0)
            {
                OffsetPosition = 0;
            }
            FillWaveForm();
            // ggggg
        }

        /// <summary>
        /// MouseWheel events handler for wave forms
        /// Magnification adjustment
        /// </summary>
        private void WaveFormsGroup_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pointer = e.GetCurrentPoint(this); 
            Point point = pointer.Position;
            // calculates offset for samples at pointer
            int offsetAtPointer = this.PointerOffsetPosition(point.X);
            PointerPointProperties prop = pointer.Properties;
            int delta = prop.MouseWheelDelta;
            if (delta > 0)
                this.MagnifyMore();
            else
                this.MagnifyLess();
            // set pointer at the same position
            this.SetOffsetForPointer(offsetAtPointer, point.X);
        }

        private void WaveFormsGroup_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint p = e.GetCurrentPoint(this); // waveFormsGroup);
            this.PointerLastPosition = p.Position;
            this.IsMovingByMouse = true;
        }

        private void WaveFormsGroup_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            this.IsMovingByMouse = false;
        }

        private void WaveFormsGroup_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (this.IsMovingByMouse)
            {
                PointerPoint p = e.GetCurrentPoint(this); // waveFormsGroup);
                PointerPointProperties prop = p.Properties;
                int shiftX = (int)(this.PointerLastPosition.X - p.Position.X);
                if (shiftX > 0)
                    this.GoNextX(Math.Abs(shiftX));
                else
                    this.GoPrevX(Math.Abs(shiftX));

                this.PointerLastPosition = p.Position;
            }
        }

        private void WaveFormsGroup_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            WaveFormsGroup_PointerReleased(sender, e);
            this.IsMovingByMouse = false;
        }

        private void WaveFormsGroup_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint p = e.GetCurrentPoint(this); // this); // waveFormsGroup);
            PointerPointProperties prop = p.Properties;
            if (prop.IsLeftButtonPressed)
                WaveFormsGroup_PointerPressed(sender, e);
        }

    }
}
