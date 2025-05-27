using Azure.Messaging.ServiceBus;
using EmailAPI.Message;
using EmailAPI.Models.Dtos;
using EmailAPI.Services;
using Newtonsoft.Json;
using System.Text;

namespace EmailAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
	private readonly string serviceBusConnectionString;
	private readonly string emailCartQueue;
	private readonly string registerUserQueue;
	private readonly IConfiguration _configuration;
	private readonly EmailService _emailService;
	private readonly string orderCreated_Topic;
	private readonly string orderCreated_Email_Subcription;

	private ServiceBusProcessor _emailCartProcessor;
	private ServiceBusProcessor _registerUserProcessor;
	private ServiceBusProcessor _emailOrderPlacedProcessor;

	public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
	{
		_configuration = configuration;
		_emailService = emailService;

		serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

		emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
		registerUserQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");
		orderCreated_Topic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
		orderCreated_Email_Subcription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_RewardsSubscription");

		var client = new ServiceBusClient(serviceBusConnectionString);
		_emailCartProcessor = client.CreateProcessor(emailCartQueue);
		_registerUserProcessor = client.CreateProcessor(registerUserQueue);
		_emailOrderPlacedProcessor = client.CreateProcessor(orderCreated_Topic, orderCreated_Email_Subcription);
	}

	public async Task Start()
	{
		_emailCartProcessor.ProcessMessageAsync += OnEmailRequestReceived;
		_emailCartProcessor.ProcessErrorAsync += ErrorHandler;
		await _emailCartProcessor.StartProcessingAsync();

		_registerUserProcessor.ProcessMessageAsync += OnUserRegisterRequestReceived;
		_registerUserProcessor.ProcessErrorAsync += ErrorHandler;
		await _registerUserProcessor.StartProcessingAsync();

		_emailOrderPlacedProcessor.ProcessMessageAsync += OnOrderPlacedRequestReceived;
		_emailOrderPlacedProcessor.ProcessErrorAsync += ErrorHandler;
		await _emailOrderPlacedProcessor.StartProcessingAsync();
	}

	public async Task Stop()
	{
		await _emailCartProcessor.StopProcessingAsync();
		await _emailCartProcessor.DisposeAsync();

		await _registerUserProcessor.StopProcessingAsync();
		await _registerUserProcessor.DisposeAsync();

		await _emailOrderPlacedProcessor.StopProcessingAsync();
		await _emailOrderPlacedProcessor.DisposeAsync();
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
			await _emailService.EmailCartAndLog(objMessage);
			await args.CompleteMessageAsync(args.Message);
		}
		catch (Exception ex)
		{
			throw;
		}
	}

	private async Task OnOrderPlacedRequestReceived(ProcessMessageEventArgs args)
	{
		// this is where you will receive message
		var message = args.Message;
		var body = Encoding.UTF8.GetString(message.Body);

		RewardsMessage objMessage = JsonConvert.DeserializeObject<RewardsMessage>(body);

		try
		{
			// TODO - try to log email
			await _emailService.LogPlacedOrder(objMessage);
			await args.CompleteMessageAsync(args.Message);
		}
		catch (Exception ex)
		{
			throw;
		}
	}

	private async Task OnUserRegisterRequestReceived(ProcessMessageEventArgs args)
	{
		var message = args.Message;
		var body = Encoding.UTF8.GetString(message.Body);

		string email = JsonConvert.DeserializeObject<string>(body);

		try
		{
			await _emailService.RegisterUserEmailAndLog(email);
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
