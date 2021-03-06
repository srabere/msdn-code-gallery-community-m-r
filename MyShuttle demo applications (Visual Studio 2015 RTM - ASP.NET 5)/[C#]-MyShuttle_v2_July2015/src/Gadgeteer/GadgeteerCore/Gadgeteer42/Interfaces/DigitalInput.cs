////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Gadgeteer.Interfaces
{
    using System;
    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;
    using Gadgeteer.Modules;

    internal class NativeDigitalInput : Socket.SocketInterfaces.DigitalInput
    {
        /// <summary>
        /// Represents the input port for the digital input.
        /// </summary>
        private InputPort _port;

        public NativeDigitalInput(Socket socket, Socket.Pin pin, GlitchFilterMode glitchFilterMode, ResistorMode resistorMode, Module module, Cpu.Pin cpuPin)
        {
            if (cpuPin == Cpu.Pin.GPIO_NONE)
            {
                // this is a mainboard error but should not happen since we check for this, but it doesnt hurt to double-check
                throw Socket.InvalidSocketException.FunctionalityException(socket, "DigitalInput");
            }

            _port = new InputPort(cpuPin, glitchFilterMode == GlitchFilterMode.On, (Port.ResistorMode)resistorMode);
        }

        public override bool Read()
        {
            return _port.Read();
        }

        public override void Dispose()
        {
            _port.Dispose();
        }
    }


    /// <summary>
    /// Represents a digital input on a single pin.
    /// </summary>
    public class DigitalInput
    {
        internal readonly Socket.SocketInterfaces.DigitalInput Interface;

        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <param name="socket">The socket for the digital input interface.</param>
        /// <param name="pin">The pin used by the digital input interface.</param>
        /// <param name="glitchFilterMode">
        ///  A value from the <see cref="GlitchFilterMode"/> enumeration that specifies 
        ///  whether to enable the glitch filter on this digital input interface.
        /// </param>
        /// <param name="resistorMode">
        ///  A value from the <see cref="ResistorMode"/> enumeration that establishes a default state for the digital input interface. N.B. .NET Gadgeteer mainboards are only required to support ResistorMode.PullUp on interruptable GPIOs and are never required to support ResistorMode.PullDown; consider putting the resistor on the module itself.
        /// </param>
        /// <param name="module">The module using this interface, which can be null if unspecified.</param>
        public DigitalInput(Socket socket, Socket.Pin pin, GlitchFilterMode glitchFilterMode, ResistorMode resistorMode, Module module)
        {
            Cpu.Pin reservedPin = socket.ReservePin(pin, module);

            // native implementation is preferred to an indirected one
            if (reservedPin == Cpu.Pin.GPIO_NONE && socket.DigitalInputIndirector != null)
                Interface = socket.DigitalInputIndirector(socket, pin, glitchFilterMode, resistorMode, module);

            else
                Interface = new NativeDigitalInput(socket, pin, glitchFilterMode, resistorMode, module, reservedPin);
        }

        /// <summary>
        /// Reads a Boolean value at the interface port input. 
        /// </summary>
        /// <returns>A Boolean value that represents the current value of the port, either 0 or 1.</returns>
        public bool Read()
        {
            return Interface.Read();
        }

        /// <summary>
        /// Returns the <see cref="Socket.SocketInterfaces.DigitalInput" /> for a <see cref="DigitalInput" /> interface.
        /// </summary>
        /// <param name="this">An instance of <see cref="DigitalInput" />.</param>
        /// <returns>The <see cref="Socket.SocketInterfaces.DigitalInput" /> for <paramref name="this"/>.</returns>
        public static explicit operator Socket.SocketInterfaces.DigitalInput(DigitalInput @this)
        {
            return @this.Interface;
        }

        /// <summary>
        /// Releases all resources used by the interface.
        /// </summary>
        public void Dispose()
        {
            Interface.Dispose();
        }
    }
}
