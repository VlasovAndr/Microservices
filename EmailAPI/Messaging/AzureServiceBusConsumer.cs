using Azure.Messaging.ServiceBus;
using EmailAPI.Models.Dtos;
using Newtonsoft.Json;
using System.Text;

namespace EmailAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
	private readonly string serviceBusConnectionString;
	private readonly string emailCartQueue;
	private readonly IConfiguration _configuration;

	private ServiceBusProcessor _emailCartProcessor;

	public AzureServiceBusConsumer(IConfiguration configuration)
	{
		_configuration = configuration;
		serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
		emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");

		var client = new ServiceBusClient(serviceBusConnectionString);
		_emailCartProcessor = client.CreateProcessor(emailCartQueue);
	}

	public async Task Start()
	{
		_emailCartProcessor.ProcessMessageAsync += OnEmailRequestReceived;
		_emailCartProcessor.ProcessErrorAsync += ErrorHandler;
	}

	public async Task Stop()
	{
		await _emailCartProcessor.StopProcessingAsync();
		await _emailCartProcessor.DisposeAsync();
	}

	private async Task OnEmailRequestReceived(ProcessMessageEventArgs args)
	{
		// this is where you will receive message
		var message = args.Message;
		var body = Encoding.UTF8.GetString(message.Body);

		CartDto objMessage = JsonConvert.DeserializeObject<CartDto>(body);

		try
		{
			// TODO - try to log email
			await args.CompleteMessageAsync(args.Message);

		}
		catch (Exception ex)
		{
			throw;
		}
	}

	private Task ErrorHandler(ProcessErrorEventArgs args)
	{
		Console.WriteLine(args.Exception.ToString());
		return Task.CompletedTask;
	}

}
