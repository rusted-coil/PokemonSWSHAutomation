using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;

namespace PokemonAutomation
{
	public class AsyncMacroTask
	{
		public enum OperationType
		{
			PressButton,
			Wait,
		}

		public class Operation
		{
			public Operation(OperationType type, int arg)
			{
				Type = type;
				Arg = arg;
			}

			public OperationType Type { get; }
			public int Arg { get; }

			void PressButton(SerialPort serialPort, int button)
			{
				if (serialPort.IsOpen)
				{
					byte[] data = new byte[2];
					data[0] = (byte)button;
					data[1] = (byte)PokemonAutomation.ButtonState.PRESS;

					serialPort.Write(data, 0, 2);
				}
			}

			void ReleaseButton(SerialPort serialPort, int button)
			{
				if (serialPort.IsOpen)
				{
					byte[] data = new byte[2];
					data[0] = (byte)button;
					data[1] = (byte)PokemonAutomation.ButtonState.RELEASE;

					serialPort.Write(data, 0, 2);
				}
			}

			public async void Execute(SerialPort serialPort)
			{
				switch (Type)
				{
					case OperationType.PressButton:
						PressButton(serialPort, Arg);
						await Task.Delay(50);
						ReleaseButton(serialPort, Arg);
						break;

					case OperationType.Wait:
						await Task.Delay(Arg);
						break;
				}
			}
		}

		// 入力するシリアルポート
		SerialPort m_SerialPort;

		// 初期化
		public AsyncMacroTask(SerialPort serialPort)
		{
			m_SerialPort = serialPort;
		}

		public async void StartAsyncMacro(IEnumerable<Operation> taskList, CancellationToken cancellationToken, bool isLoop)
		{
			await Task.Run(async () =>
			{
				do
				{
					if (cancellationToken.IsCancellationRequested)
					{
						return;
					}
					foreach (var task in taskList)
					{
						task.Execute(m_SerialPort);
					}
				} while (isLoop);
			}, cancellationToken);
		}
	}
}
