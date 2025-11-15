export type ExampleCollectionDto = {
  id: string;
  created: string;
  name: string;
};

export type ExampleDetailsDto = {
  id: string;
  name: string;
  description: string;
};

export type ExampleCreatedEvent = {
  correlationId: string;
  id: string;
  name: string;
  description: string;
};

export type ExampleUpdatedEvent = {
  correlationId: string;
  id: string;
  description: string;
};
