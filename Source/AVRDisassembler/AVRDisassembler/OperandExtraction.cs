﻿using System;
using System.Collections.Generic;
using AVRDisassembler.InstructionSet.OpCodes;
using AVRDisassembler.InstructionSet.OpCodes.Arithmetic;
using AVRDisassembler.InstructionSet.OpCodes.Bits;
using AVRDisassembler.InstructionSet.OpCodes.Branch;
using AVRDisassembler.InstructionSet.Operands;

namespace AVRDisassembler
{
    public static class OperandExtraction
    {
        public static IEnumerable<IOperand> ExtractOperands(Type type, byte[] bytes)
        {
            #region Instructions
            if (   type == typeof(ADC)
                || type == typeof(ADD)
                || type == typeof(AND)
               )
            {
                var vals = new[] { bytes[0], bytes[1] }.MapToMask("------rd ddddrrrr");
                yield return new Operand(OperandType.DestinationRegister, vals['d']);
                yield return new Operand(OperandType.SourceRegister, vals['r']);
                yield break;
            }
            if (type == typeof(ADIW))
            {
                var vals = new[] { bytes[0], bytes[1] }.MapToMask("-------- KKddKKKK");
                var r = 0;
                switch (vals['d'])
                {
                    case 0: r = 24; break;
                    case 1: r = 26; break;
                    case 2: r = 28; break;
                    case 3: r = 30; break;
                }
                yield return new Operand(OperandType.DestinationRegister, r);
                yield return new Operand(OperandType.ConstantData, vals['K']);
                yield break;
            }
            if (type == typeof(ANDI))
            {
                var vals = new[] { bytes[0], bytes[1] }.MapToMask("----KKKK ddddKKKK");
                yield return new Operand(OperandType.DestinationRegister, 16 + vals['d']);
                yield return new Operand(OperandType.ConstantData, vals['K']);
                yield break;
            }
            if (type == typeof(ASR))
            {
                var vals = new[] { bytes[0], bytes[1] }.MapToMask("-------d dddd----");
                yield return new Operand(OperandType.DestinationRegister, vals['d']);
                yield break;
            }
            if (type == typeof(BCLR))
            {
                var vals = new[] { bytes[0], bytes[1] }.MapToMask("-------- -sss----");
                yield return new Operand(OperandType.StatusRegisterBit, vals['s']);
                yield break;
            }
            if (type == typeof(BLD))
            {
                var vals = new[] { bytes[0], bytes[1] }.MapToMask("-------d dddd-bbb");
                yield return new Operand(OperandType.DestinationRegister, vals['d']);
                yield return new Operand(OperandType.BitRegisterIO, vals['b']);
                yield break;
            }
            if (   type == typeof(BRBC)
                || type == typeof(BRBS)
               )
            {
                var vals = new[] { bytes[0], bytes[1] }.MapToMask("------kk kkkkksss");
                yield return new Operand(OperandType.StatusRegisterBit, vals['s']);
                yield return new Operand(OperandType.ConstantAddress, CalculateTwosComplement(vals['k'], 7));
                yield break;
            }
            if (type == typeof(JMP))
            {
                // To save a bit, the address is shifted on to the right prior to storing (this works because jumps are 
                // always on even boundaries). The MCU knows this, so shifts the addrss one to the left when loading it.
                var vals = new[] { bytes[2], bytes[3] }.MapToMask("kkkkkkkk kkkkkkkk");
                var addressVal = vals['k'] << 1;
                yield return new Operand(OperandType.ConstantAddress, addressVal);
                yield break;
            }

            #endregion

            #region Pseudoinstructions

            if (type == typeof(DATA))
            {
                yield return new Operand(OperandType._RawData, bytes);
                yield break;
            }

            #endregion
        }

        public static int CalculateTwosComplement(int val, int numberOfBits)
        {
            var mask = 1 << numberOfBits - 1;
            return -(val & mask) + (val & ~mask);
        }
    }
}
