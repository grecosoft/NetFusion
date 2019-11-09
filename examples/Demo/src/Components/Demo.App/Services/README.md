Components containing domain-logic based on multiple domain-entities or logic that cannot
be easily represented/implemented in the domain-model.  If the logic being implemented is
dependent on other services/repositories, it is best implemented as a service into which
the domain-entities are passed.