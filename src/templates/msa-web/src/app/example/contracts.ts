export interface ExampleCollectionDto {
  //id: string;
  position: number;
  name: string;
}

export interface ExampleDetailsDto {
  name: string;
  position: number;
  weight: number;
  symbol: string;
}

export interface IExampleCreatedEvent {
  correlationId: string;
  id: string;
}

export interface IExampleUpdatedEvent {
  correlationId: string;
  id: string;
}
