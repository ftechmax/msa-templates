export interface ExampleCollectionDto {
  id: string;
  name: string;
}

export interface ExampleDetailsDto {
  name: string;
  description: string;
}

export interface IExampleCreatedEvent {
  correlationId: string;
  id: string;
}

export interface IExampleUpdatedEvent {
  correlationId: string;
  id: string;
}
